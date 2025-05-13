using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class FeatureHandler : AbstractChangeHandler<GameFeature, FeatureDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public FeatureHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new FeatureMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(GameFeature oldEntity, GameFeature entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.FeatureIds != null && g.FeatureIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.FeatureIds.Remove(oldEntity.Id);
                game.FeatureIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Feature?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Feature.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Feature.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override GameFeature CreateNewInstance()
        {
            return new GameFeature();
        }

        protected override IItemCollection<GameFeature> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Features;
        }

        protected override Task<FeatureDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetFeature(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Feature;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, FeatureDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.FeatureIds != null && g.FeatureIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Feature?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}