using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class GenreSynchronizer : AbstractSynchronizer<Genre>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public GenreSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Genre> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Genres;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Genre genre))
            {
                return;
            }

            Logger.Trace($"Saving genre with id = {genre.Id}");
            var savedObject = await _syncBackendService.SaveGenre(genre, cancellationToken);
            if (savedObject.Id != genre.Id.ToString())
            {
                Logger.Debug($"Genre id mismatch: saved id = {savedObject.Id}, entity id = {genre.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Genre genre))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting genre with id = {genre.Id}");
            return _syncBackendService.DeleteGenre(genre, cancellationToken);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Genre;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Genre");
        }
    }
}