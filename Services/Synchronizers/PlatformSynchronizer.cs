using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class PlatformSynchronizer : AbstractDiffSynchronizer<Platform>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public PlatformSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Platform> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Platforms;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Platform platform))
            {
                return;
            }

            Logger.Trace($"Saving platform with id = {platform.Id}");
            var savedObject = await _syncBackendService.SavePlatform(platform, cancellationToken);
            if (savedObject.Id != platform.Id.ToString())
            {
                Logger.Debug($"Platform id mismatch: saved id = {savedObject.Id}, entity id = {platform.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override async Task SaveDiffObject(object oldEntity, object newEntity,
            CancellationToken cancellationToken = default)
        {
            if (!(oldEntity is Platform oldPlatform))
            {
                return;
            }

            if (!(newEntity is Platform newPlatform))
            {
                return;
            }

            Logger.Trace(
                $"Saving platform diff with id = {newPlatform.Id}");
            var savedObject = await _syncBackendService.SavePlatformDiff(oldPlatform, newPlatform, cancellationToken);
            if (savedObject.Id != newPlatform.Id.ToString())
            {
                Logger.Debug($"Platform id mismatch: saved id = {savedObject.Id}, entity id = {newPlatform.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Platform platform))
            {
                return Task.CompletedTask;
            }

            Logger.Trace(
                $"Deleting platform with id = {platform.Id}");
            return _syncBackendService.DeletePlatform(platform, cancellationToken);
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