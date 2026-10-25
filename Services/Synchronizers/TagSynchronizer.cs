using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class TagSynchronizer : AbstractSynchronizer<Tag>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public TagSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Tag> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Tags;
        }


        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Tag tag))
            {
                return;
            }

            Logger.Trace($"Saving tag with id = {tag.Id}");
            var savedObject = await _syncBackendService.SaveTag(tag, cancellationToken);
            if (savedObject.Id != tag.Id.ToString())
            {
                Logger.Debug($"Tag id mismatch: saved id = {savedObject.Id}, entity id = {tag.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Tag tag))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting tag with id = {tag.Id}");
            return _syncBackendService.DeleteTag(tag, cancellationToken);
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