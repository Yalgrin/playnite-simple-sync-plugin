using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class AgeRatingSynchronizer : AbstractSynchronizer<AgeRating>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public AgeRatingSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<AgeRating> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.AgeRatings;
        }

        protected override Task SaveObject(AgeRating entity)
        {
            Logger.Trace($"Saving age rating with id = {entity.Id}");
            return _syncBackendService.SaveAgeRating(entity);
        }

        protected override Task DeleteObject(AgeRating entity)
        {
            Logger.Trace($"Deleting age rating with id = {entity.Id}");
            return _syncBackendService.DeleteAgeRating(entity);
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