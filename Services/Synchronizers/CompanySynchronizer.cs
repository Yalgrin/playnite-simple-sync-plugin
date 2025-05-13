using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class CompanySynchronizer : AbstractSynchronizer<Company>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CompanySynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Company> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Companies;
        }

        protected override Task SaveObject(Company entity)
        {
            Logger.Trace($"Saving company with id = {entity.Id}");
            return _syncBackendService.SaveCompany(entity);
        }

        protected override Task DeleteObject(Company entity)
        {
            Logger.Trace($"Deleting company with id = {entity.Id}");
            return _syncBackendService.DeleteCompany(entity);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Company;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Company");
        }
    }
}