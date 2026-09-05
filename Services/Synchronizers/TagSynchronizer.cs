using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class TagSynchronizer : AbstractSynchronizer<Tag>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public TagSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Tag> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Tags;
        }

        protected override Task SaveObject(Tag entity, CancellationToken cancellationToken = default)
        {
            Logger.Trace($"Saving tag with id = {entity.Id}");
            return _syncBackendService.SaveTag(entity, cancellationToken);
        }

        protected override Task DeleteObject(Tag entity, CancellationToken cancellationToken = default)
        {
            Logger.Trace($"Deleting tag with id = {entity.Id}");
            return _syncBackendService.DeleteTag(entity, cancellationToken);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Tag;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Tag");
        }
    }
}