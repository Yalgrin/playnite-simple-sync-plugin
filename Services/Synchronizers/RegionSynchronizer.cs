using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class RegionSynchronizer : AbstractSynchronizer<Region>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public RegionSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Region> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Regions;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Region region))
            {
                return;
            }

            Logger.Trace($"Saving region with id = {region.Id}");
            var savedObject = await _syncBackendService.SaveRegion(region, cancellationToken);
            if (savedObject.Id != region.Id.ToString())
            {
                Logger.Debug($"Region id mismatch: saved id = {savedObject.Id}, entity id = {region.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Region region))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting category with id = {region.Id}");
            return _syncBackendService.DeleteRegion(region, cancellationToken);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Region;
        }

        protected override bool HasObjectChanged(ItemUpdateEvent<Region> args)
        {
            var newData = args.NewData;
            var oldData = args.OldData;
            return newData != null && (oldData == null || oldData.Id != newData.Id || oldData.Name != newData.Name ||
                                       oldData.SpecificationId != newData.SpecificationId);
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Region");
        }
    }
}