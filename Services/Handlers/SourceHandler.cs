using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class SourceHandler : AbstractChangeHandler<GameSource, SourceDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public SourceHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new SourceMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(GameSource oldEntity, GameSource entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.SourceId == oldEntity.Id))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.SourceId = entity.Id;
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Source?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Source.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Source.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override GameSource CreateNewInstance()
        {
            return new GameSource();
        }

        protected override IItemCollection<GameSource> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Sources;
        }

        protected override Task<SourceDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetSource(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Source;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, SourceDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return db.Games.All(g => g.SourceId != guid) && !db.FilterPresets.Any(g =>
            {
                var contains = g?.Settings?.Source?.Ids?.Contains(guid);
                return contains != null && (bool)contains;
            });
        }
    }
}