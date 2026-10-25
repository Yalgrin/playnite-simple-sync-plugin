using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class SourceSynchronizer : AbstractSynchronizer<GameSource>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public SourceSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<GameSource> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Sources;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is GameSource source))
            {
                return;
            }

            Logger.Trace($"Saving source with id = {source.Id}");
            var savedObject = await _syncBackendService.SaveSource(source, cancellationToken);
            if (savedObject.Id != source.Id.ToString())
            {
                Logger.Debug($"Source id mismatch: saved id = {savedObject.Id}, entity id = {source.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is GameSource source))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting source with id = {source.Id}");
            return _syncBackendService.DeleteSource(source, cancellationToken);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Source;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Source");
        }
    }
}