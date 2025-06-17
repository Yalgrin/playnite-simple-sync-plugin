using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Mappers;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Services.Handlers
{
    public class GameHandler : AbstractChangeHandler<Game, GameDto>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public GameHandler(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataSynchronizationService dataSynchronizationService) : base(api, new GameMapper(api),
            dataSynchronizationService)
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

        protected override Task<GameDto> GetObject(ChangeDto dto)
        {
            return _syncBackendService.SyncBackendClient.GetGame(dto.ObjectId);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Game;
        }

        protected override bool CanRemove(IGameDatabaseAPI db, GameDto objectDto)
        {
            return true;
        }

        protected override bool HasEntityBeenChanged(Game entity, GameDto objectDto)
        {
            return (objectDto.HasIcon || entity.Icon != null)
                   || (objectDto.HasCoverImage || entity.CoverImage != null)
                   || (objectDto.HasBackgroundImage || entity.BackgroundImage != null)
                   || (entity.Name != objectDto.Name)
                   || (entity.Description != objectDto.Description)
                   || (entity.Notes != objectDto.Notes)
                   || (HaveItemsChanged(entity.Genres, objectDto.Genres))
                   || (entity.Hidden != objectDto.Hidden)
                   || (entity.Favorite != objectDto.Favorite)
                   || (entity.LastActivity != objectDto.LastActivity)
                   || (entity.SortingName != objectDto.SortingName)
                   || (entity.GameId != objectDto.GameId)
                   || (entity.PluginId.ToString() != objectDto.PluginId)
                   || (HaveItemsChanged(entity.Platforms, objectDto.Platforms))
                   || (HaveItemsChanged(entity.Publishers, objectDto.Publishers))
                   || (HaveItemsChanged(entity.Developers, objectDto.Developers))
                   || (entity.ReleaseDate?.Date != objectDto.ReleaseDate)
                   || (HaveItemsChanged(entity.Categories, objectDto.Categories))
                   || (HaveItemsChanged(entity.Tags, objectDto.Tags))
                   || (HaveItemsChanged(entity.Features, objectDto.Features))
                   || (HaveLinksChanged(entity.Links, objectDto.Links))
                   || (entity.Playtime != objectDto.Playtime)
                   || (entity.Added != objectDto.Added)
                   || (entity.Modified != objectDto.Modified)
                   || (entity.PlayCount != objectDto.PlayCount)
                   || (entity.InstallSize != objectDto.InstallSize)
                   || (entity.LastSizeScanDate != objectDto.LastSizeScanDate)
                   || (HaveItemsChanged(entity.Series, objectDto.Series))
                   || (entity.Version != objectDto.Version)
                   || (HaveItemsChanged(entity.AgeRatings, objectDto.AgeRatings))
                   || (HaveItemsChanged(entity.Regions, objectDto.Regions))
                   || (entity.SourceId.ToString() != objectDto.Source?.Id)
                   || (entity.CompletionStatusId.ToString() != objectDto.CompletionStatus?.Id)
                   || (entity.UserScore != objectDto.UserScore)
                   || (entity.CriticScore != objectDto.CriticScore)
                   || (entity.CommunityScore != objectDto.CommunityScore)
                   || (entity.Manual != objectDto.Manual);
        }

        private static bool HaveItemsChanged<T, U>(List<T> oldItems, List<U> newItems)
            where T : DatabaseObject where U : AbstractDto
        {
            var oldItemsCount = oldItems?.Count ?? 0;
            var newItemsCount = newItems?.Count ?? 0;
            return oldItemsCount != newItemsCount || (oldItems != null && newItems != null
                                                                       && !oldItems.All(t =>
                                                                           newItems.Any(i => i.Id == t.Id.ToString())));
        }

        private static bool HaveLinksChanged(Collection<Link> oldItems, List<LinkDto> newItems)
        {
            var oldItemsCount = oldItems?.Count ?? 0;
            var newItemsCount = newItems?.Count ?? 0;
            if (oldItemsCount != newItemsCount)
            {
                return true;
            }

            if (oldItems == null || newItems == null)
            {
                return false;
            }

            for (var i = 0; i < oldItems.Count; i++)
            {
                var oldLink = oldItems[i];
                var newLink = newItems[i];
                if (oldLink.Name != newLink.Name || oldLink.Url != newLink.Url)
                {
                    return true;
                }
            }

            return false;
        }

        protected override async Task HandleMetadata(IGameDatabaseAPI db, Game entity, GameDto objectDto,
            long dtoObjectId)
        {
            var loadIconTask = LoadMetadata(db, entity, dtoObjectId, objectDto.HasIcon, "Icon", entity.Icon,
                str => entity.Icon = str);
            var loadCoverTask = LoadMetadata(db, entity, dtoObjectId, objectDto.HasCoverImage, "CoverImage",
                entity.CoverImage,
                str => entity.CoverImage = str);
            var loadBackgroundTask = LoadMetadata(db, entity, dtoObjectId, objectDto.HasBackgroundImage,
                "BackgroundImage",
                entity.BackgroundImage, str => entity.BackgroundImage = str);

            await loadIconTask;
            await loadCoverTask;
            await loadBackgroundTask;
        }

        protected override Task<Tuple<byte[], string>> GetObjectMetadata(long dtoObjectId, string metadataName)
        {
            return _syncBackendService.SyncBackendClient.GetGameMetadata(dtoObjectId, metadataName);
        }

        protected override bool NeedsToHandleMetadata(Game entity, GameDto objectDto)
        {
            return true;
        }

        protected override Game FindDatabaseObject(IItemCollection<Game> databaseCollection, GameDto objectDto,
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
                    oldEntity = entity;
                    reassignIds = true;

                    entity = oldEntity.GetCopy();
                    entity.Id = new Guid(objectDto.Id);
                    isNew = true;
                }
            }
            else
            {
                entity = CreateNewInstance();
                isNew = true;
            }

            return entity;
        }

        protected override bool FillEntityPostNewEntitySave(Game entity, GameDto dto)
        {
            if (dto.Added != null)
            {
                entity.Added = dto.Added;
            }

            if (dto.Modified != null)
            {
                entity.Modified = dto.Modified;
            }

            return true;
        }
    }
}