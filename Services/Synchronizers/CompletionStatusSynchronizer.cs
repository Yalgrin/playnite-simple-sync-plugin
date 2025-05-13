using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class CompletionStatusSynchronizer : AbstractSynchronizer<CompletionStatus>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CompletionStatusSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<CompletionStatus> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.CompletionStatuses;
        }

        protected override Task SaveObject(CompletionStatus entity)
        {
            Logger.Trace($"Saving completion status with id = {entity.Id}");
            return _syncBackendService.SaveCompletionStatus(entity);
        }

        protected override Task DeleteObject(CompletionStatus entity)
        {
            Logger.Trace($"Deleting completion status with id = {entity.Id}");
            return _syncBackendService.DeleteCompletionStatus(entity);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.CompletionStatus;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_CompletionStatus");
        }
    }
}