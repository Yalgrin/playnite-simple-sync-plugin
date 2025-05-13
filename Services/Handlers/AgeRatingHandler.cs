using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class AgeRatingHandler : AbstractChangeHandler<AgeRating, AgeRatingDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public AgeRatingHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new AgeRatingMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(AgeRating oldEntity, AgeRating entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.AgeRatingIds != null && g.AgeRatingIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.AgeRatingIds.Remove(oldEntity.Id);
                game.AgeRatingIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.AgeRating?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.AgeRating.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.AgeRating.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override AgeRating CreateNewInstance()
        {
            return new AgeRating();
        }

        protected override IItemCollection<AgeRating> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.AgeRatings;
        }

        protected override Task<AgeRatingDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetAgeRating(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.AgeRating;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, AgeRatingDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.AgeRatingIds != null && g.AgeRatingIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.AgeRating?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}