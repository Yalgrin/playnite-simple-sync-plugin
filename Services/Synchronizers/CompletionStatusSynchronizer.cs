using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class CompletionStatusSynchronizer : AbstractSynchronizer<CompletionStatus>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CompletionStatusSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<CompletionStatus> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.CompletionStatuses;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is CompletionStatus completionStatus))
            {
                return;
            }

            Logger.Trace($"Saving completion status with id = {completionStatus.Id}");
            var savedObject = await _syncBackendService.SaveCompletionStatus(completionStatus, cancellationToken);
            if (savedObject.Id != completionStatus.Id.ToString())
            {
                Logger.Debug(
                    $"Completion status id mismatch: saved id = {savedObject.Id}, entity id = {completionStatus.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is CompletionStatus completionStatus))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting completion status with id = {completionStatus.Id}");
            return _syncBackendService.DeleteCompletionStatus(completionStatus, cancellationToken);
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