using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class SeriesHandler : AbstractChangeHandler<Series, SeriesDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public SeriesHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new SeriesMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(Series oldEntity, Series entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.SeriesIds != null && g.SeriesIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.SeriesIds.Remove(oldEntity.Id);
                game.SeriesIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Series?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Series.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Series.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override Series CreateNewInstance()
        {
            return new Series();
        }

        protected override IItemCollection<Series> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Series;
        }

        protected override Task<SeriesDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetSeries(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Series;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, SeriesDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.SeriesIds != null && g.SeriesIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Series?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}