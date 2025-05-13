using System;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Exceptions;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services
{
    public class SyncBackendService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SimpleSyncPluginSettingsViewModel _settingsViewModel;
        private readonly object _lock = new object();

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

        public SyncBackendClient SyncBackendClient { get; private set; }
        public Guid ClientId { get; private set; }

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

            var settings = _settingsViewModel.Settings;
            lock (_lock)
            {
                if (settings.SynchronizationEnabled && settings.SyncServerAddress != null)
                {
                    ClientId = Guid.NewGuid();
                    SyncBackendClient = new SyncBackendClient(api, settings.SyncServerAddress, ClientId);
                    Logger.Info(
                        $"Prepared a sync client with address {settings.SyncServerAddress} and client id {ClientId}");
                }
            }

            _settingsViewModel.PropertyChanged += (sender, args) =>
            {
                lock (_lock)
                {
                    var simpleSyncPluginSettings = _settingsViewModel.Settings;
                    if (simpleSyncPluginSettings.SynchronizationEnabled &&
                        simpleSyncPluginSettings.SyncServerAddress != null)
                    {
                        if (SyncBackendClient == null || simpleSyncPluginSettings.SyncServerAddress !=
                            SyncBackendClient.ServerAddress)
                        {
                            SyncBackendClient?.Shutdown();
                            ClientId = Guid.NewGuid();
                            SyncBackendClient = new SyncBackendClient(api, simpleSyncPluginSettings.SyncServerAddress,
                                ClientId);
                            Logger.Info(
                                $"Prepared a sync client with address {settings.SyncServerAddress} and client id {ClientId}");
                        }
                    }
                    else
                    {
                        SyncBackendClient?.Shutdown();
                        SyncBackendClient = null;
                        Logger.Info("Cleared the sync client");
                    }
                }
            };
        }

        public async Task SaveCategory(Category category)
        {
            var dto = _categoryMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveCategory(dto);
            }
        }

        public async Task DeleteCategory(Category category)
        {
            var dto = _categoryMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteCategory(dto);
            }
        }

        public async Task SaveGenre(Genre category)
        {
            var dto = _genreMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveGenre(dto);
            }
        }

        public async Task DeleteGenre(Genre category)
        {
            var dto = _genreMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteGenre(dto);
            }
        }

        public async Task SavePlatform(Platform category)
        {
            var dto = _platformMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SavePlatform(dto, category.Icon, category.Cover, category.Background);
            }
        }

        public async Task SavePlatformDiff(Platform oldEntity, Platform newEntity)
        {
            var dto = _platformDiffMapper.ToDiffDto(oldEntity, newEntity);
            if (SyncBackendClient != null)
            {
                try
                {
                    await SyncBackendClient.SavePlatformDiff(dto, newEntity.Icon, newEntity.Cover,
                        newEntity.Background);
                }
                catch (ManualSynchronizationRequiredException ex)
                {
                    Logger.Error(ex, "ManualSynchronizationRequiredException");
                    await SavePlatform(newEntity);
                }
            }
        }

        public async Task DeletePlatform(Platform category)
        {
            var dto = _platformMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeletePlatform(dto);
            }
        }

        public async Task SaveCompany(Company category)
        {
            var dto = _companyMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveCompany(dto);
            }
        }

        public async Task DeleteCompany(Company category)
        {
            var dto = _companyMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteCompany(dto);
            }
        }

        public async Task SaveFeature(GameFeature category)
        {
            var dto = _featureMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveFeature(dto);
            }
        }

        public async Task DeleteFeature(GameFeature category)
        {
            var dto = _featureMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteFeature(dto);
            }
        }

        public async Task SaveTag(Tag category)
        {
            var dto = _tagMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveTag(dto);
            }
        }

        public async Task DeleteTag(Tag category)
        {
            var dto = _tagMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteTag(dto);
            }
        }

        public async Task SaveSeries(Series category)
        {
            var dto = _seriesMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveSeries(dto);
            }
        }

        public async Task DeleteSeries(Series category)
        {
            var dto = _seriesMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteSeries(dto);
            }
        }

        public async Task SaveAgeRating(AgeRating category)
        {
            var dto = _ageRatingMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveAgeRating(dto);
            }
        }

        public async Task DeleteAgeRating(AgeRating category)
        {
            var dto = _ageRatingMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteAgeRating(dto);
            }
        }

        public async Task SaveRegion(Region category)
        {
            var dto = _regionMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveRegion(dto);
            }
        }

        public async Task DeleteRegion(Region category)
        {
            var dto = _regionMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteRegion(dto);
            }
        }

        public async Task SaveSource(GameSource category)
        {
            var dto = _sourceMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveSource(dto);
            }
        }

        public async Task DeleteSource(GameSource category)
        {
            var dto = _sourceMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteSource(dto);
            }
        }

        public async Task SaveCompletionStatus(CompletionStatus category)
        {
            var dto = _completionStatusMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveCompletionStatus(dto);
            }
        }

        public async Task DeleteCompletionStatus(CompletionStatus category)
        {
            var dto = _completionStatusMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteCompletionStatus(dto);
            }
        }

        public async Task SaveFilterPreset(FilterPreset category)
        {
            var dto = _filterPresetMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveFilterPreset(dto);
            }
        }

        public async Task DeleteFilterPreset(FilterPreset category)
        {
            var dto = _filterPresetMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteFilterPreset(dto);
            }
        }

        public async Task SaveGame(Game category)
        {
            var dto = _gameMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.SaveGame(dto, category.Icon, category.CoverImage, category.BackgroundImage);
            }
        }

        public async Task SaveGameDiff(Game oldEntity, Game newEntity)
        {
            var dto = _gameDiffMapper.ToDiffDto(oldEntity, newEntity);
            if (SyncBackendClient != null)
            {
                try
                {
                    await SyncBackendClient.SaveGameDiff(dto, newEntity.Icon, newEntity.CoverImage,
                        newEntity.BackgroundImage);
                }
                catch (ManualSynchronizationRequiredException ex)
                {
                    Logger.Error(ex, "ManualSynchronizationRequiredException");
                    await SaveGame(newEntity);
                }
            }
        }

        public async Task DeleteGame(Game category)
        {
            var dto = _gameMapper.ToDto(category);
            if (SyncBackendClient != null)
            {
                await SyncBackendClient.DeleteGame(dto);
            }
        }
    }
}