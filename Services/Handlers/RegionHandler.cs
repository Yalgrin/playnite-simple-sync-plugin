using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class RegionHandler : AbstractChangeHandler<Region, RegionDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public RegionHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new RegionMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(Region oldEntity, Region entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.RegionIds != null && g.RegionIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.RegionIds.Remove(oldEntity.Id);
                game.RegionIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Region?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Region.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Region.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override Region CreateNewInstance()
        {
            return new Region();
        }

        protected override IItemCollection<Region> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Regions;
        }

        protected override Task<RegionDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetRegion(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Region;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, RegionDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.RegionIds != null && g.RegionIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Region?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}