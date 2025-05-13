using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class PlatformSynchronizer : AbstractDiffSynchronizer<Platform>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public PlatformSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Platform> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Platforms;
        }

        protected override Task SaveObject(Platform entity)
        {
            Logger.Trace($"Saving platform with id = {entity.Id}");
            return _syncBackendService.SavePlatform(entity);
        }

        protected override Task SaveDiffObject(Platform oldEntity, Platform newEntity)
        {
            Logger.Trace($"Saving platform diff with id = {newEntity.Id}");
            return _syncBackendService.SavePlatformDiff(oldEntity, newEntity);
        }

        protected override Task DeleteObject(Platform entity)
        {
            Logger.Trace($"Deleting platform with id = {entity.Id}");
            return _syncBackendService.DeletePlatform(entity);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Platform;
        }

        protected override bool HasObjectChanged(ItemUpdateEvent<Platform> args)
        {
            var newData = args.NewData;
            var oldData = args.OldData;
            return newData != null && (oldData == null || oldData.Id != newData.Id || oldData.Name != newData.Name ||
                                       oldData.SpecificationId != newData.SpecificationId ||
                                       oldData.Icon != newData.Icon || oldData.Cover != newData.Cover ||
                                       oldData.Background != newData.Background);
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Platform");
        }
    }
}