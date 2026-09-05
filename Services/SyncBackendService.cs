using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Exceptions;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;
using static SimpleSyncPlugin.Commons.MessageConstants;

namespace SimpleSyncPlugin.Services
{
    public class SyncBackendService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SimpleSyncPluginSettingsViewModel _settingsViewModel;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private readonly CategoryMapper _categoryMapper;
        private readonly GenreMapper _genreMapper;
        private readonly PlatformMapper _platformMapper;
        private readonly PlatformDiffMapper _platformDiffMapper;
        private readonly CompanyMapper _companyMapper;
        private readonly FeatureMapper _featureMapper;
        private readonly TagMapper _tagMapper;
        private readonly SeriesMapper _seriesMapper;
        private readonly AgeRatingMapper _ageRatingMapper;
        private readonly RegionMapper _regionMapper;
        private readonly SourceMapper _sourceMapper;
        private readonly CompletionStatusMapper _completionStatusMapper;
        private readonly FilterPresetMapper _filterPresetMapper;
        private readonly GameMapper _gameMapper;
        private readonly GameDiffMapper _gameDiffMapper;
        private readonly IPlayniteAPI _api;

        public SyncBackendClient SyncBackendClient { get; private set; }

        public SyncBackendService(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel)
        {
            _settingsViewModel = settingsViewModel;

            _categoryMapper = new CategoryMapper();
            _genreMapper = new GenreMapper();
            _platformMapper = new PlatformMapper();
            _platformDiffMapper = new PlatformDiffMapper();
            _companyMapper = new CompanyMapper();
            _featureMapper = new FeatureMapper();
            _tagMapper = new TagMapper();
            _seriesMapper = new SeriesMapper();
            _ageRatingMapper = new AgeRatingMapper();
            _regionMapper = new RegionMapper();
            _sourceMapper = new SourceMapper();
            _completionStatusMapper = new CompletionStatusMapper();
            _filterPresetMapper = new FilterPresetMapper();
            _gameMapper = new GameMapper(api);
            _gameDiffMapper = new GameDiffMapper(api);
            _api = api;

            var settings = _settingsViewModel.Settings;
            _lock.Wait();
            try
            {
                if (settings.SynchronizationEnabled && settings.SyncServerAddress != null &&
                    !string.IsNullOrEmpty(_settingsViewModel.ClientInfo.ClientId))
                {
                    var clientInfo = _settingsViewModel.ClientInfo.Clone();
                    SyncBackendClient = new SyncBackendClient(api, settings.SyncServerAddress, clientInfo);
                    Logger.Info(
                        $"Prepared a sync client with address {settings.SyncServerAddress} and client id {clientInfo.ClientId}");
                }
            }
            finally
            {
                _lock.Release();
            }

            _settingsViewModel.PropertyChanged += async (sender, args) =>
            {
                await _lock.WaitAsync();
                try
                {
                    var simpleSyncPluginSettings = _settingsViewModel.Settings;
                    var registeredClientInfo = _settingsViewModel.ClientInfo;
                    if (simpleSyncPluginSettings.SynchronizationEnabled &&
                        simpleSyncPluginSettings.SyncServerAddress != null &&
                        !string.IsNullOrEmpty(registeredClientInfo.ClientId))
                    {
                        if (SyncBackendClient == null || simpleSyncPluginSettings.SyncServerAddress !=
                            SyncBackendClient.ServerAddress ||
                            registeredClientInfo.ClientId != SyncBackendClient.ClientInfo.ClientId
                            || registeredClientInfo.ClientToken != SyncBackendClient.ClientInfo.ClientToken)
                        {
                            SyncBackendClient?.Shutdown();
                            var clientInfo = registeredClientInfo.Clone();
                            SyncBackendClient = new SyncBackendClient(api, simpleSyncPluginSettings.SyncServerAddress,
                                clientInfo);
                            Logger.Info(
                                $"Prepared a sync client with address {settings.SyncServerAddress} and client id {clientInfo.ClientId}");
                        }
                        else
                        {
                            try
                            {
                                if (SessionManager.CurrentSession?.SessionId == null)
                                {
                                    return;
                                }

                                if (_settingsViewModel.Settings.FetchLiveChanges)
                                {
                                    Logger.Info("Enabling change stream from settings change");
                                    await (SyncBackendClient?.EnableChangeStream() ?? Task.CompletedTask);
                                }
                                else
                                {
                                    Logger.Info("Disabling change stream from settings change");
                                    await (SyncBackendClient?.DisableChangeStream() ?? Task.CompletedTask);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Error while enabling/disabling change stream");
                            }
                        }
                    }
                    else
                    {
                        SyncBackendClient?.Shutdown();
                        SyncBackendClient = null;
                        Logger.Info("Cleared the sync client");
                    }
                }
                finally
                {
                    _lock.Release();
                }
            };

            SessionManager.CurrentSessionChanged += async (sender, args) =>
            {
                await _lock.WaitAsync();
                try
                {
                    if (SessionManager.CurrentSession?.SessionId == null)
                    {
                        return;
                    }

                    if (_settingsViewModel.Settings.FetchLiveChanges)
                    {
                        Logger.Info("Enabling change stream from session change");
                        await (SyncBackendClient?.EnableChangeStream() ?? Task.CompletedTask);
                    }
                    else
                    {
                        Logger.Info("Disabling change stream from session change");
                        await (SyncBackendClient?.DisableChangeStream() ?? Task.CompletedTask);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while enabling/disabling change stream");
                }
                finally
                {
                    _lock.Release();
                }
            };
        }

        public Task<CheckResultDto> CheckConnection(CancellationToken cancellationToken = default)
        {
            if (SyncBackendClient == null)
            {
                return Task.FromResult<CheckResultDto>(null);
            }

            return HandleRequest(SyncBackendClient.CheckConnection(cancellationToken));
        }

        public async Task SaveCategory(Category category, CancellationToken cancellationToken = default)
        {
            var dto = _categoryMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveCategory(dto, cancellationToken));
            }
        }

        public async Task DeleteCategory(Category category, CancellationToken cancellationToken = default)
        {
            var dto = _categoryMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteCategory(dto, cancellationToken));
            }
        }

        public async Task SaveGenre(Genre category, CancellationToken cancellationToken = default)
        {
            var dto = _genreMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveGenre(dto, cancellationToken));
            }
        }

        public async Task DeleteGenre(Genre category, CancellationToken cancellationToken = default)
        {
            var dto = _genreMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteGenre(dto, cancellationToken));
            }
        }

        public async Task SavePlatform(Platform category, CancellationToken cancellationToken = default)
        {
            var dto = _platformMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SavePlatform(dto, category.Icon, category.Cover,
                    category.Background, cancellationToken));
            }
        }

        public async Task SavePlatformDiff(Platform oldEntity, Platform newEntity,
            CancellationToken cancellationToken = default)
        {
            var dto = _platformDiffMapper.ToDiffDto(oldEntity, newEntity);
            if (SyncBackendClient != null)
            {
                try
                {
                    await HandleRequest(SyncBackendClient.SavePlatformDiff(dto, newEntity.Icon, newEntity.Cover,
                        newEntity.Background, cancellationToken));
                }
                catch (ManualSynchronizationRequiredException ex)
                {
                    Logger.Error(ex, "ManualSynchronizationRequiredException");
                    await HandleRequest(SavePlatform(newEntity, cancellationToken));
                }
            }
        }

        public async Task DeletePlatform(Platform category, CancellationToken cancellationToken = default)
        {
            var dto = _platformMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeletePlatform(dto, cancellationToken));
            }
        }

        public async Task SaveCompany(Company category, CancellationToken cancellationToken = default)
        {
            var dto = _companyMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveCompany(dto, cancellationToken));
            }
        }

        public async Task DeleteCompany(Company category, CancellationToken cancellationToken = default)
        {
            var dto = _companyMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteCompany(dto, cancellationToken));
            }
        }

        public async Task SaveFeature(GameFeature category, CancellationToken cancellationToken = default)
        {
            var dto = _featureMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveFeature(dto, cancellationToken));
            }
        }

        public async Task DeleteFeature(GameFeature category, CancellationToken cancellationToken = default)
        {
            var dto = _featureMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteFeature(dto, cancellationToken));
            }
        }

        public async Task SaveTag(Tag category, CancellationToken cancellationToken = default)
        {
            var dto = _tagMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveTag(dto, cancellationToken));
            }
        }

        public async Task DeleteTag(Tag category, CancellationToken cancellationToken = default)
        {
            var dto = _tagMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteTag(dto, cancellationToken));
            }
        }

        public async Task SaveSeries(Series category, CancellationToken cancellationToken = default)
        {
            var dto = _seriesMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveSeries(dto, cancellationToken));
            }
        }

        public async Task DeleteSeries(Series category, CancellationToken cancellationToken = default)
        {
            var dto = _seriesMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteSeries(dto, cancellationToken));
            }
        }

        public async Task SaveAgeRating(AgeRating category, CancellationToken cancellationToken = default)
        {
            var dto = _ageRatingMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveAgeRating(dto, cancellationToken));
            }
        }

        public async Task DeleteAgeRating(AgeRating category, CancellationToken cancellationToken = default)
        {
            var dto = _ageRatingMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteAgeRating(dto, cancellationToken));
            }
        }

        public async Task SaveRegion(Region category, CancellationToken cancellationToken = default)
        {
            var dto = _regionMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveRegion(dto, cancellationToken));
            }
        }

        public async Task DeleteRegion(Region category, CancellationToken cancellationToken = default)
        {
            var dto = _regionMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteRegion(dto, cancellationToken));
            }
        }

        public async Task SaveSource(GameSource category, CancellationToken cancellationToken = default)
        {
            var dto = _sourceMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveSource(dto, cancellationToken));
            }
        }

        public async Task DeleteSource(GameSource category, CancellationToken cancellationToken = default)
        {
            var dto = _sourceMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteSource(dto, cancellationToken));
            }
        }

        public async Task SaveCompletionStatus(CompletionStatus category, CancellationToken cancellationToken = default)
        {
            var dto = _completionStatusMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveCompletionStatus(dto, cancellationToken));
            }
        }

        public async Task DeleteCompletionStatus(CompletionStatus category,
            CancellationToken cancellationToken = default)
        {
            var dto = _completionStatusMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteCompletionStatus(dto, cancellationToken));
            }
        }

        public async Task SaveFilterPreset(FilterPreset category, CancellationToken cancellationToken = default)
        {
            var dto = _filterPresetMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveFilterPreset(dto, cancellationToken));
            }
        }

        public async Task DeleteFilterPreset(FilterPreset category, CancellationToken cancellationToken = default)
        {
            var dto = _filterPresetMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteFilterPreset(dto, cancellationToken));
            }
        }

        public async Task SaveGame(Game category, CancellationToken cancellationToken = default)
        {
            var dto = _gameMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.SaveGame(dto, category.Icon, category.CoverImage,
                    category.BackgroundImage, cancellationToken));
            }
        }

        public async Task SaveGameDiff(Game oldEntity, Game newEntity, CancellationToken cancellationToken = default)
        {
            var dto = _gameDiffMapper.ToDiffDto(oldEntity, newEntity);
            if (SyncBackendClient != null)
            {
                try
                {
                    await HandleRequest(SyncBackendClient.SaveGameDiff(dto, newEntity.Icon, newEntity.CoverImage,
                        newEntity.BackgroundImage, cancellationToken));
                }
                catch (ManualSynchronizationRequiredException ex)
                {
                    Logger.Error(ex, "ManualSynchronizationRequiredException");
                    await HandleRequest(SaveGame(newEntity, cancellationToken));
                }
            }
        }

        public async Task DeleteGame(Game category, CancellationToken cancellationToken = default)
        {
            var dto = _gameMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await HandleRequest(SyncBackendClient.DeleteGame(dto, cancellationToken));
            }
        }

        public async Task<List<ChangeMessage>> FetchAll(CancellationToken cancellationToken = default)
        {
            if (SyncBackendClient == null)
            {
                return new List<ChangeMessage>();
            }

            return await HandleRequest(SyncBackendClient.FetchAll(cancellationToken));
        }

        public async Task<List<ChangeMessage>> FetchRemainingChanges(long lastProcessedId,
            CancellationToken cancellationToken = default)
        {
            if (SyncBackendClient == null)
            {
                return new List<ChangeMessage>();
            }

            return await HandleRequest(SyncBackendClient.FetchRemainingChanges(lastProcessedId, cancellationToken));
        }

        private Task HandleRequest(Task requestTask)
        {
            return HandleRequest(Task.Run<object>(async () =>
            {
                await requestTask;
                return null;
            }));
        }

        private async Task<T> HandleRequest<T>(Task<T> requestTask)
        {
            try
            {
                return await requestTask;
            }
            catch (ForceFetchRequiredException ex)
            {
                Logger.Error(ex, $"Force fetch required for object!");
                _api.Notifications.Add(new NotificationMessage(ForceFetchRequiredId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ForceFetchRequired"), NotificationType.Error));
                throw;
            }
            catch (HttpStatusException ex)
            {
                Logger.Error(ex, $"Request failed!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpStatusError"), ex.StatusCode,
                        ex.Message), NotificationType.Error));
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.Error(ex, $"Request failed!");
                _api.Notifications.Add(new NotificationMessage(HttpErrorId,
                    string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_HttpError"), ex.Message),
                    NotificationType.Error));
                throw;
            }
            catch (OperationCanceledException e)
            {
                Logger.Warn($"Request canceled: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Request failed!");
                _api.Notifications.Add(new NotificationMessage(ClientErrorId,
                    GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_UnexpectedError"), NotificationType.Error));
                throw;
            }
        }

        private string GetLocalizedString(string key)
        {
            return _api.GetLocalizedString(key);
        }
    }
}