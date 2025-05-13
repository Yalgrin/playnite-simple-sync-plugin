using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class GenreHandler : AbstractChangeHandler<Genre, GenreDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public GenreHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new GenreMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(Genre oldEntity, Genre entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.GenreIds != null && g.GenreIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.GenreIds.Remove(oldEntity.Id);
                game.GenreIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Genre?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Genre.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Genre.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override Genre CreateNewInstance()
        {
            return new Genre();
        }

        protected override IItemCollection<Genre> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Genres;
        }

        protected override Task<GenreDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetGenre(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Genre;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, GenreDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.GenreIds != null && g.GenreIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Genre?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}