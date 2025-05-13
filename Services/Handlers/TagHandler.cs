using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class TagHandler : AbstractChangeHandler<Tag, TagDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public TagHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new TagMapper(),
            dataSynchronizationService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override void HandleReassigningIds(Tag oldEntity, Tag entity, IGameDatabaseAPI db)
        {
            foreach (var game in db.Games.Where(g => g.TagIds != null && g.TagIds.Contains(oldEntity.Id)))
            {
                Logger.Info($"Modifying game {game.Id}...");
                game.TagIds.Remove(oldEntity.Id);
                game.TagIds.Add(entity.Id);
                _dataSynchronizationService.RegisterGracePeriod(ObjectType.Game, game.Id);
                db.Games.Update(game);
            }

            foreach (var filterPreset in db.FilterPresets)
            {
                var contains = filterPreset?.Settings?.Tag?.Ids?.Contains(oldEntity.Id);
                if (contains != null && (bool)contains)
                {
                    Logger.Info($"Modifying filter preset {filterPreset.Id}...");
                    filterPreset.Settings.Tag.Ids.Remove(oldEntity.Id);
                    filterPreset.Settings.Tag.Ids.Add(entity.Id);
                    _dataSynchronizationService.RegisterGracePeriod(ObjectType.FilterPreset, filterPreset.Id);
                    db.FilterPresets.Update(filterPreset);
                }
            }
        }

        protected override Tag CreateNewInstance()
        {
            return new Tag();
        }

        protected override IItemCollection<Tag> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Tags;
        }

        protected override Task<TagDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetTag(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Tag;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, TagDto objectDto)
        {
            var guid = new Guid(objectDto.Id);
            return !db.Games.Any(g => g.TagIds != null && g.TagIds.Contains(guid))
                   && !db.FilterPresets.Any(g =>
                   {
                       var contains = g?.Settings?.Tag?.Ids?.Contains(guid);
                       return contains != null && (bool)contains;
                   });
        }
    }
}