using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class GameDiffHandler : AbstractDiffChangeHandler<Game, GameDto, GameDiffDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public GameDiffHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService, GameHandler gameHandler) : base(api,
            new GameDiffMapper(api), dataSynchronizationService, gameHandler)
        {
            _syncBackendService = syncBackendService;
        }

        protected override Game CreateNewInstance()
        {
            return new Game();
        }

        protected override IItemCollection<Game> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Games;
        }

        protected override Task<GameDiffDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetGameDiff(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.GameDiff;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, GameDiffDto objectDto)
        {
            return true;
        }

        protected override bool HasEntityBeenChanged(Game entity, GameDiffDto objectDto)
        {
            return true;
        }

        protected override async Task HandleMetadata(IGameDatabaseAPI db, Game entity, GameDiffDto objectDto,
            long dtoObjectId)
        {
            var changeFields = objectDto.ChangedFields;
            if (changeFields == null)
            {
                return;
            }

            var loadIconTask = LoadMetadata(db, entity, objectDto, changeFields, "Icon", entity.Icon,
                str => entity.Icon = str);
            var loadCoverTask = LoadMetadata(db, entity, objectDto, changeFields, "CoverImage", entity.CoverImage,
                str => entity.CoverImage = str);
            var loadBackgroundTask = LoadMetadata(db, entity, objectDto, changeFields, "BackgroundImage",
                entity.BackgroundImage, str => entity.BackgroundImage = str);

            await loadIconTask;
            await loadCoverTask;
            await loadBackgroundTask;
        }

        protected override Task<Tuple<byte[], string>> GetObjectMetadata(long dtoObjectId, string metadataName)
        {
            return _syncBackendService.SyncBackendClient.GetGameMetadata(dtoObjectId, metadataName);
        }

        protected override bool NeedsToHandleMetadata(Game entity, GameDiffDto objectDto)
        {
            var changeFields = objectDto.ChangedFields;
            return changeFields != null && (changeFields.Contains("Icon") || changeFields.Contains("CoverImage") ||
                                            changeFields.Contains("BackgroundImage"));
        }

        protected override Game FindDatabaseObject(IItemCollection<Game> databaseCollection, GameDiffDto objectDto,
            ref Game oldEntity, ref bool reassignIds,
            ref bool isNew)
        {
            Game entity;
            var matchingEntities = databaseCollection
                .Where(e => e.GameId == objectDto.GameId && e.PluginId.ToString() == objectDto.PluginId).ToList();
            if (matchingEntities.Count > 0)
            {
                entity = matchingEntities[0];
                if (entity.Id.ToString() != objectDto.Id)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return entity;
        }

        protected override bool FillEntityPostNewEntitySave(Game entity, GameDiffDto dto)
        {
            var changeFields = dto.ChangedFields;
            bool changedAnything = false;
            if (changeFields != null)
            {
                if (dto.Added != null && changeFields.Contains("Added"))
                {
                    entity.Added = dto.Added;
                    changedAnything = true;
                }

                if (dto.Modified != null && changeFields.Contains("Modified"))
                {
                    entity.Modified = dto.Modified;
                    changedAnything = true;
                }
            }

            return changedAnything;
        }

        protected override Task<GameDto> GetFullObject(long objectId)
        {
            return _syncBackendService.SyncBackendClient.GetGame(objectId);
        }
    }
}