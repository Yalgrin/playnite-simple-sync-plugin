using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class SeriesSynchronizer : AbstractSynchronizer<Series>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public SeriesSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Series> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Series;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Series series))
            {
                return;
            }

            Logger.Trace($"Saving series with id = {series.Id}");
            var savedObject = await _syncBackendService.SaveSeries(series, cancellationToken);
            if (savedObject.Id != series.Id.ToString())
            {
                Logger.Debug($"Series id mismatch: saved id = {savedObject.Id}, entity id = {series.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Series series))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting series with id = {series.Id}");
            return _syncBackendService.DeleteSeries(series, cancellationToken);
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