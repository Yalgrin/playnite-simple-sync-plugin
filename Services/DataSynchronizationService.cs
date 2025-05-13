using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Services.Synchronizers;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services
{
    public class DataSynchronizationService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly IPlayniteAPI _api;
        private readonly SimpleSyncPluginSettingsViewModel _settingsViewModel;
        private readonly SyncBackendService _syncBackendService;
        private readonly List<IObjectSynchronizer> _synchronizerList;
        private readonly Dictionary<ObjectType, IObjectSynchronizer> _objectSynchronizers;

        public DataSynchronizationService(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService)
        {
            _api = api;
            _settingsViewModel = settingsViewModel;
            _syncBackendService = syncBackendService;
            _synchronizerList = CreateSynchronizerList();
            _objectSynchronizers = CreateSynchronizerMap();
        }

        private List<IObjectSynchronizer> CreateSynchronizerList()
        {
            return new List<IObjectSynchronizer>
            {
                new CategorySynchronizer(_api, _settingsViewModel, _syncBackendService),
                new GenreSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new PlatformSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new CompanySynchronizer(_api, _settingsViewModel, _syncBackendService),
                new FeatureSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new TagSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new SeriesSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new AgeRatingSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new RegionSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new SourceSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new CompletionStatusSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new FilterPresetSynchronizer(_api, _settingsViewModel, _syncBackendService),
                new GameSynchronizer(_api, _settingsViewModel, _syncBackendService)
            };
        }

        private Dictionary<ObjectType, IObjectSynchronizer> CreateSynchronizerMap()
        {
            return _synchronizerList.ToDictionary(handler => handler.GetHandledType());
        }

        public void RegisterListeners()
        {
            foreach (var pair in _objectSynchronizers)
            {
                pair.Value.RegisterListeners();
            }
        }

        public async Task SyncAll(GlobalProgressActionArgs progArgs)
        {
            try
            {
                Logger.Info("SyncAll > START");

                var db = _api.Database;

                var elementCount = _synchronizerList.Sum(synchronizer => synchronizer.GetElementCount(db));

                Logger.Info($"SyncAll > items to sync: {elementCount}");
                progArgs.ProgressMaxValue = elementCount;
                progArgs.CurrentProgressValue = 0;

                foreach (var synchronizer in _synchronizerList)
                {
                    if (progArgs.CancelToken.IsCancellationRequested)
                    {
                        Logger.Info("SyncAll > END, cancel requested...");
                        return;
                    }

                    await synchronizer.SyncAll(db, progArgs);
                }

                Logger.Info("SyncAll > END");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while syncing objects!");
            }
        }

        public async Task SyncGames(GlobalProgressActionArgs progArgs, List<Guid> ids)
        {
            try
            {
                Logger.Info($"SyncGames > START, ids.Count: {ids.Count}");

                var db = _api.Database;
                if (_objectSynchronizers.TryGetValue(ObjectType.Game, out var gameSynchronizer))
                {
                    progArgs.ProgressMaxValue = ids.Count;
                    progArgs.CurrentProgressValue = 0;
                    await gameSynchronizer.SyncSelected(db, progArgs, ids);
                }

                Logger.Info("SyncGames > END");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error occurred while syncing objects!");
            }
        }

        public void RegisterGracePeriod(ObjectType objectType, Guid id)
        {
            if (_objectSynchronizers.TryGetValue(objectType.GetBaseObjectType(), out var synchronizer))
            {
                synchronizer.RegisterGracePeriod(id);
            }
        }
    }
}