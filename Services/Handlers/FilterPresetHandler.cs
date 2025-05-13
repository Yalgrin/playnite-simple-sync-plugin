using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class FilterPresetHandler : AbstractChangeHandler<FilterPreset, FilterPresetDto>
    {
        private readonly SyncBackendService _syncBackendService;

        public FilterPresetHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new FilterPresetMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override FilterPreset CreateNewInstance()
        {
            return new FilterPreset();
        }

        protected override IItemCollection<FilterPreset> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.FilterPresets;
        }

        protected override Task<FilterPresetDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetFilterPreset(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.FilterPreset;
        }
    }
}