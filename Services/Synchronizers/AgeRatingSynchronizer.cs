using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class AgeRatingSynchronizer : AbstractSynchronizer<AgeRating>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public AgeRatingSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<AgeRating> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.AgeRatings;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is AgeRating ageRating))
            {
                return;
            }

            Logger.Trace($"Saving age rating with id = {ageRating.Id}");
            var savedObject = await _syncBackendService.SaveAgeRating(ageRating, cancellationToken);
            if (savedObject.Id != ageRating.Id.ToString())
            {
                Logger.Debug($"Age rating id mismatch: saved id = {savedObject.Id}, entity id = {ageRating.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is AgeRating ageRating))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting age rating with id = {ageRating.Id}");
            return _syncBackendService.DeleteAgeRating(ageRating, cancellationToken);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.AgeRating;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_AgeRating");
        }
    }
}