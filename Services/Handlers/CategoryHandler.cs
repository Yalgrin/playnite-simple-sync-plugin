using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class CategoryHandler : AbstractChangeHandler<Category, CategoryDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CategoryHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new CategoryMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(Category oldEntity, Category entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.CategoryIds != null && g.CategoryIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.CategoryIds.Remove(oldEntity.Id);
                game.CategoryIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Category?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Category.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Category.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override Category CreateNewInstance()
        {
            return new Category();
        }

        protected override IItemCollection<Category> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Categories;
        }

        protected override Task<CategoryDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetCategory(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Category;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, CategoryDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.CategoryIds != null && g.CategoryIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Category?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}