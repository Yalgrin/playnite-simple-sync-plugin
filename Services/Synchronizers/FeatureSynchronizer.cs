using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class FeatureSynchronizer : AbstractSynchronizer<GameFeature>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public FeatureSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<GameFeature> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Features;
        }

        protected override Task SaveObject(GameFeature entity)
        {
            Logger.Trace($"Saving feature with id = {entity.Id}");
            return _syncBackendService.SaveFeature(entity);
        }

        protected override Task DeleteObject(GameFeature entity)
        {
            Logger.Trace($"Deleting feature with id = {entity.Id}");
            return _syncBackendService.DeleteFeature(entity);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Feature;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Feature");
        }
    }
}