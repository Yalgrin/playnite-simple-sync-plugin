using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class SourceSynchronizer : AbstractSynchronizer<GameSource>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public SourceSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<GameSource> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Sources;
        }

        protected override Task SaveObject(GameSource entity, CancellationToken cancellationToken = default)
        {
            Logger.Trace($"Saving source with id = {entity.Id}");
            return _syncBackendService.SaveSource(entity, cancellationToken);
        }

        protected override Task DeleteObject(GameSource entity, CancellationToken cancellationToken = default)
        {
            Logger.Trace($"Deleting source with id = {entity.Id}");
            return _syncBackendService.DeleteSource(entity, cancellationToken);
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