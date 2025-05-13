using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class PlatformDiffHandler : AbstractDiffChangeHandler<Platform, PlatformDto, PlatformDiffDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public PlatformDiffHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService, PlatformHandler platformHandler) : base(api,
            new PlatformDiffMapper(), dataSynchronizationService, platformHandler)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(Platform oldEntity, Platform entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.PlatformIds != null && g.PlatformIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.PlatformIds.Remove(oldEntity.Id);
                game.PlatformIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Platform?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Platform.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Platform.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override Platform CreateNewInstance()
        {
            return new Platform();
        }

        protected override IItemCollection<Platform> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Platforms;
        }

        protected override Task<PlatformDiffDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetPlatformDiff(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.PlatformDiff;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, PlatformDiffDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.PlatformIds != null && g.PlatformIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Platform?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }

        protected override bool HasEntityBeenChanged(Platform entity, PlatformDiffDto objectDto)
        {
            return true;
        }

        protected override async Task HandleMetadata(IGameDatabaseAPI db, Platform entity, PlatformDiffDto objectDto,
            long dtoObjectId)
        {
            var changeFields = objectDto.ChangedFields;
            if (changeFields == null)
            {
                return;
            }

            var loadIconTask = LoadMetadata(db, entity, objectDto, changeFields, "Icon", entity.Icon,
                str => entity.Icon = str);
            var loadCoverTask = LoadMetadata(db, entity, objectDto, changeFields, "CoverImage", entity.Cover,
                str => entity.Cover = str);
            var loadBackgroundTask = LoadMetadata(db, entity, objectDto, changeFields, "BackgroundImage",
                entity.Background, str => entity.Background = str);

            await loadIconTask;
            await loadCoverTask;
            await loadBackgroundTask;
        }


        protected override Task<Tuple<byte[], string>> GetObjectMetadata(long dtoObjectId, string metadataName)
        {
            return _syncBackendService.SyncBackendClient.GetPlatformMetadata(dtoObjectId, metadataName);
        }

        protected override bool NeedsToHandleMetadata(Platform entity, PlatformDiffDto objectDto)
        {
            var changeFields = objectDto.ChangedFields;
            return changeFields != null && (changeFields.Contains("Icon") || changeFields.Contains("CoverImage") ||
                                            changeFields.Contains("BackgroundImage"));
        }

        protected override Task<PlatformDto> GetFullObject(long objectId)
        {
            return _syncBackendService.SyncBackendClient.GetPlatform(objectId);
        }
    }
}