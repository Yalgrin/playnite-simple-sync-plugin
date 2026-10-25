using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class FeatureSynchronizer : AbstractSynchronizer<GameFeature>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public FeatureSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<GameFeature> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Features;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is GameFeature feature))
            {
                return;
            }

            Logger.Trace($"Saving feature with id = {feature.Id}");
            var savedObject = await _syncBackendService.SaveFeature(feature, cancellationToken);
            if (savedObject.Id != feature.Id.ToString())
            {
                Logger.Debug($"Feature id mismatch: saved id = {savedObject.Id}, entity id = {feature.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is GameFeature feature))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting feature with id = {feature.Id}");
            return _syncBackendService.DeleteFeature(feature, cancellationToken);
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