using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class CompanySynchronizer : AbstractSynchronizer<Company>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CompanySynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Company> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Companies;
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Company company))
            {
                return;
            }

            Logger.Trace($"Saving company with id = {company.Id}");
            var savedObject = await _syncBackendService.SaveCompany(company, cancellationToken);
            if (savedObject.Id != company.Id.ToString())
            {
                Logger.Debug($"Company id mismatch: saved id = {savedObject.Id}, entity id = {company.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Company company))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Deleting company with id = {company.Id}");
            return _syncBackendService.DeleteCompany(company, cancellationToken);
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