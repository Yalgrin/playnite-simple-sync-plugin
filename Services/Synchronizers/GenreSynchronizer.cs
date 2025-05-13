using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class GenreSynchronizer : AbstractSynchronizer<Genre>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public GenreSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Genre> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Genres;
        }

        protected override Task SaveObject(Genre entity)
        {
            Logger.Trace($"Saving genre with id = {entity.Id}");
            return _syncBackendService.SaveGenre(entity);
        }

        protected override Task DeleteObject(Genre entity)
        {
            Logger.Trace($"Deleting genre with id = {entity.Id}");
            return _syncBackendService.DeleteGenre(entity);
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