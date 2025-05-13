using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class PlatformHandler : AbstractChangeHandler<Platform, PlatformDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public PlatformHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new PlatformMapper(),
            dataSynchronizationService)
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

        protected override Task<PlatformDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetPlatform(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Platform;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, PlatformDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.PlatformIds != null && g.PlatformIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Platform?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }

        protected override bool HasEntityBeenChanged(Platform entity, PlatformDto objectDto)
        {
            return (objectDto.HasIcon || entity.Icon != null) || (objectDto.HasCoverImage || entity.Cover != null) ||
                   (objectDto.HasBackgroundImage || entity.Background != null)
                   || (entity.Name != objectDto.Name) || (entity.SpecificationId != objectDto.SpecificationId);
        }

        protected override async Task HandleMetadata(IGameDatabaseAPI db, Platform entity, PlatformDto objectDto,
            long dtoObjectId)
        {
            var loadIconTask = LoadMetadata(db, entity, dtoObjectId, objectDto.HasIcon, "Icon", entity.Icon,
                str => entity.Icon = str);
            var loadCoverTask = LoadMetadata(db, entity, dtoObjectId, objectDto.HasCoverImage, "CoverImage",
                entity.Cover, str => entity.Cover = str);
            var loadBackgroundTask = LoadMetadata(db, entity, dtoObjectId, objectDto.HasBackgroundImage,
                "BackgroundImage", entity.Background, str => entity.Background = str);

            await loadIconTask;
            await loadCoverTask;
            await loadBackgroundTask;
        }

        protected override Task<Tuple<byte[], string>> GetObjectMetadata(long dtoObjectId, string metadataName)
        {
            return _syncBackendService.SyncBackendClient.GetPlatformMetadata(dtoObjectId, metadataName);
        }

        protected override bool NeedsToHandleMetadata(Platform entity, PlatformDto objectDto)
        {
            return true;
        }
    }
}