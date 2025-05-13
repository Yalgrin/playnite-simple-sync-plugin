using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class CategorySynchronizer : AbstractSynchronizer<Category>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CategorySynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Category> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Categories;
        }

        protected override Task SaveObject(Category entity)
        {
            Logger.Trace($"Saving category with id = {entity.Id}");
            return _syncBackendService.SaveCategory(entity);
        }

        protected override Task DeleteObject(Category entity)
        {
            Logger.Trace($"Deleting category with id = {entity.Id}");
            return _syncBackendService.DeleteCategory(entity);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Category;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Category");
        }
    }
}