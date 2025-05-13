using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class CompanyHandler : AbstractChangeHandler<Company, CompanyDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CompanyHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new CompanyMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(Company oldEntity, Company entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games)
            {
                var containsDev = game.DeveloperIds != null && game.DeveloperIds.Contains(oldEntity.Id);
                var containsPub = game.PublisherIds != null && game.PublisherIds.Contains(oldEntity.Id);
                if (containsDev)
                {
                    game.DeveloperIds.Remove(oldEntity.Id);
                    game.DeveloperIds.Add(entity.Id);
                }

                if (containsPub)
                {
                    game.PublisherIds.Remove(oldEntity.Id);
                    game.PublisherIds.Add(entity.Id);
                }

                if (containsDev || containsPub)
                {
                    Logger.Info($"Modifying game {game.Id}...");
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                    db.Games.Update(game);
                }
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var containsDev = filterPreset?.Settings?.Developer?.Ids?.Contains(oldEntity.Id) ?? false;
                var containsPub = filterPreset?.Settings?.Publisher?.Ids?.Contains(oldEntity.Id) ?? false;

                if (containsDev)
                {
                    filterPreset.Settings.Developer.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Developer.Ids.Add(entity.Id);
                }

                if (containsPub)
                {
                    filterPreset.Settings.Publisher.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Publisher.Ids.Add(entity.Id);
                }

                if (containsDev || containsPub)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override Company CreateNewInstance()
        {
            return new Company();
        }

        protected override IItemCollection<Company> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Companies;
        }

        protected override Task<CompanyDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetCompany(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Company;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, CompanyDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g =>
                       (g.DeveloperIds != null && g.DeveloperIds.Contains(guid)) ||
                       (g.PublisherIds != null && g.PublisherIds.Contains(guid)))
                   && !db.FilterPresets.Any(g =>
                   {
                       var containsDev = g?.Settings?.Developer?.Ids?.Contains(guid);
                       var containsPub = g?.Settings?.Publisher?.Ids?.Contains(guid);
                       return (containsDev != null && (bool)containsDev) || (containsPub != null && (bool)containsPub);
                   });
        }
    }
}