using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class CompletionStatusHandler : AbstractChangeHandler<CompletionStatus, CompletionStatusDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public CompletionStatusHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api,
            new CompletionStatusMapper(), dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(CompletionStatus oldEntity, CompletionStatus entity,
            IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.CompletionStatusId == oldEntity.Id))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.CompletionStatusId = entity.Id;
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.CompletionStatuses?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.CompletionStatuses.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.CompletionStatuses.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }

            var completionStatusSettings = _api.ApplicationSettings.CompletionStatus;
            if (completionStatusSettings.DefaultStatus == oldEntity.Id ||
                completionStatusSettings.PlayedStatus == oldEntity.Id)
            {
                Logger.Info("Statuses have changed, make sure to manually set the defaults!");
                _api.Notifications.Add(new NotificationMessage("Yalgrin-SimpleSyncPlugin-ReassignStatuses",
                    "SimpleSyncPlugin - statuses have changed, make sure to manually set the defaults!",
                    NotificationType.Error));
            }
        }

        protected override CompletionStatus CreateNewInstance()
        {
            return new CompletionStatus();
        }

        protected override IItemCollection<CompletionStatus> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.CompletionStatuses;
        }

        protected override Task<CompletionStatusDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetCompletionStatus(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.CompletionStatus;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, CompletionStatusDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            var completionStatusSettings = _api.ApplicationSettings.CompletionStatus;
            return completionStatusSettings.DefaultStatus != guid && completionStatusSettings.PlayedStatus != guid &&
                   db.Games.All(g => g.CompletionStatusId != guid) && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.CompletionStatuses?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}