using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class SeriesSynchronizer : AbstractSynchronizer<Series>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public SeriesSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Series> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Series;
        }

        protected override Task SaveObject(Series entity)
        {
            Logger.Trace($"Saving series with id = {entity.Id}");
            return _syncBackendService.SaveSeries(entity);
        }

        protected override Task DeleteObject(Series entity)
        {
            Logger.Trace($"Deleting series with id = {entity.Id}");
            return _syncBackendService.DeleteSeries(entity);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Series;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Series");
        }
    }
}