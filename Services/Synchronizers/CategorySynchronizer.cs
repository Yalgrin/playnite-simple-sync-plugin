using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class CategorySynchronizer : AbstractSynchronizer<Category>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CategorySynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Category> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Categories;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Category category))
            {
                return;
            }

            Logger.Trace($"Saving category with id = {category.Id}");
            var savedObject = await _syncBackendService.SaveCategory(category, cancellationToken);
            if (savedObject.Id != category.Id.ToString())
            {
                Logger.Debug($"Category id mismatch: saved id = {savedObject.Id}, entity id = {category.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Category category))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting category with id = {category.Id}");
            return _syncBackendService.DeleteCategory(category, cancellationToken);
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