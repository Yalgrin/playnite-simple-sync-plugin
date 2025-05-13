using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class RegionSynchronizer : AbstractSynchronizer<Region>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public RegionSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Region> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Regions;
        }

        protected override Task SaveObject(Region entity)
        {
            Logger.Trace($"Saving region with id = {entity.Id}");
            return _syncBackendService.SaveRegion(entity);
        }

        protected override Task DeleteObject(Region entity)
        {
            Logger.Trace($"Deleting region with id = {entity.Id}");
            return _syncBackendService.DeleteRegion(entity);
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