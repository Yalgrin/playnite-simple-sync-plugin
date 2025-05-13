using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services.Handlers;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services
{
    public class DataProcessingService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly IPlayniteAPI _api;
        private readonly SimpleSyncPluginSettingsViewModel _settings;
        private readonly SyncBackendService _syncBackendService;
        private readonly DataSynchronizationService _dataSynchronizationService;
        private readonly List<IChangeHandler> _changeHandlerList;
        private readonly Dictionary<ObjectType, IChangeHandler> _changeHandlers;

        public DataProcessingService(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settings,
            SyncBackendService syncBackendService, DataSynchronizationService dataSynchronizationService)
        {
            _api = api;
            _settings = settings;
            _syncBackendService = syncBackendService;
            _dataSynchronizationService = dataSynchronizationService;
            _changeHandlerList = CreateHandlerList();
            _changeHandlers = CreateHandlers();
        }

        private List<IChangeHandler> CreateHandlerList()
        {
            var platformHandler = new PlatformHandler(_api, _syncBackendService, _dataSynchronizationService);
            var gameHandler = new GameHandler(_api, _syncBackendService, _dataSynchronizationService);
            return new List<IChangeHandler>
            {
                new CategoryHandler(_api, _syncBackendService, _dataSynchronizationService),
                new GenreHandler(_api, _syncBackendService, _dataSynchronizationService),
                platformHandler,
                new PlatformDiffHandler(_api, _syncBackendService, _dataSynchronizationService, platformHandler),
                new CompanyHandler(_api, _syncBackendService, _dataSynchronizationService),
                new FeatureHandler(_api, _syncBackendService, _dataSynchronizationService),
                new TagHandler(_api, _syncBackendService, _dataSynchronizationService),
                new SeriesHandler(_api, _syncBackendService, _dataSynchronizationService),
                new AgeRatingHandler(_api, _syncBackendService, _dataSynchronizationService),
                new RegionHandler(_api, _syncBackendService, _dataSynchronizationService),
                new SourceHandler(_api, _syncBackendService, _dataSynchronizationService),
                new CompletionStatusHandler(_api, _syncBackendService, _dataSynchronizationService),
                new FilterPresetHandler(_api, _syncBackendService, _dataSynchronizationService),
                gameHandler,
                new GameDiffHandler(_api, _syncBackendService, _dataSynchronizationService, gameHandler)
            };
        }

        private Dictionary<ObjectType, IChangeHandler> CreateHandlers()
        {
            return _changeHandlerList.ToDictionary(handler => handler.GetHandledType());
        }

        public async Task ProcessChange(ChangeDto dto)
        {
            try
            {
                if (dto.ForceFetch || dto.ClientId != _syncBackendService.ClientId.ToString())
                {
                    _changeHandlers.TryGetValue(dto.Type, out var handler);
                    if (handler != null)
                    {
                        Logger.Trace(
                            $"Trying to process change with ID = {dto.Id} of type {dto.Type} and base object id {dto.ObjectId} (force fetch: {dto.ForceFetch})...");
                        await handler.HandleChange(dto);
                    }
                    else
                    {
                        Logger.Warn($"No handler found type {dto.Type}!");
                    }
                }
                else
                {
                    Logger.Trace(
                        $"Skipping processing of message ID = {dto.Id} of type {dto.Type} and base object id {dto.ObjectId}, it's from the current client...");
                }

                _settings.UpdateLastProcessedId(dto.Id);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while processing changes");
                throw;
            }
        }

        public async Task FetchAll(GlobalProgressActionArgs progArgs)
        {
            Logger.Info("FetchAll > START");
            var changeDtos = await _syncBackendService.SyncBackendClient.FetchAll();
            var changeCount = changeDtos.Count;
            Logger.Info($"FetchAll > found {changeCount} changes");

            progArgs.ProgressMaxValue = changeCount;
            progArgs.CurrentProgressValue = 0;

            long? id = null;
            foreach (var changeDto in changeDtos)
            {
                if (progArgs.CancelToken.IsCancellationRequested)
                {
                    Logger.Info($"FetchAll > END, cancel requested...");
                    return;
                }

                progArgs.Text = GetLocalizedString("LOC_Yalgrin_SimpleSync_Dialogs_Fetch") + "\n" +
                                (progArgs.CurrentProgressValue + 1) + "/" + changeCount;

                await ProcessChange(changeDto);
                progArgs.CurrentProgressValue++;
                if (changeDto.Id != null && (id == null || id < changeDto.Id))
                {
                    id = changeDto.Id;
                }
            }

            _settings.UpdateLastProcessedId(id);
            Logger.Info("FetchAll > END");
        }

        public async Task FetchGames(GlobalProgressActionArgs progArgs, GameChangeRequestDto requestDto)
        {
            Logger.Info("FetchGames > START");
            var changeDtos = await _syncBackendService.SyncBackendClient.FetchGames(requestDto);
            var changeCount = changeDtos.Count;
            Logger.Info($"FetchGames > found {changeCount} changes");

            progArgs.ProgressMaxValue = changeCount;
            progArgs.CurrentProgressValue = 0;

            long? id = null;
            foreach (var changeDto in changeDtos)
            {
                if (progArgs.CancelToken.IsCancellationRequested)
                {
                    Logger.Info("FetchGames > END, cancel requested...");
                    return;
                }

                progArgs.Text = GetLocalizedString("LOC_Yalgrin_SimpleSync_Dialogs_Fetch") + "\n" +
                                (progArgs.CurrentProgressValue + 1) + "/" + changeCount;

                await ProcessChange(changeDto);
                progArgs.CurrentProgressValue++;
                if (changeDto.Id != null && (id == null || id < changeDto.Id))
                {
                    id = changeDto.Id;
                }
            }

            _settings.UpdateLastProcessedId(id);
            Logger.Info("FetchGames > END");
        }

        public async Task FetchRemainingChanges(GlobalProgressActionArgs progArgs)
        {
            try
            {
                Logger.Info("FetchRemainingChanges > START");

                List<ChangeDto> changeDtos =
                    await _syncBackendService.SyncBackendClient.FetchRemainingChanges(_settings.Settings
                        .LastProcessedId);

                Logger.Info($"FetchRemainingChanges > found {changeDtos.Count} changes to process!");

                progArgs.ProgressMaxValue = changeDtos.Count;
                progArgs.CurrentProgressValue = 0;

                foreach (var changeDto in changeDtos)
                {
                    if (progArgs.CancelToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await ProcessChange(changeDto);

                    _settings.UpdateLastProcessedId(changeDto.Id);

                    progArgs.CurrentProgressValue++;
                }

                Logger.Info("FetchRemainingChanges > END");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while processing changes");
            }
        }

        private string GetLocalizedString(string key)
        {
            return _api.GetLocalizedString(key);
        }
    }
}