using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class GameSynchronizer : AbstractDiffSynchronizer<Game>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public GameSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService,
            SyncBackendService syncBackendService) : base(api, settingsViewModel, dataProcessingBroker,
            dataProcessingService)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<Game> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.Games;
        }

        protected override List<Game> GetOrderedCollection(IGameDatabaseAPI db)
        {
            return db.Games
                .OrderBy(g => g.Added ?? DateTime.MinValue)
                .ThenBy(g => g.Modified ?? DateTime.MinValue)
                .ThenBy(g => g.Name)
                .ToList();
        }

        public override async Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Game game))
            {
                return;
            }

            Logger.Trace($"Saving game with id = {game.Id}, gameId = {game.GameId}, pluginId = {game.PluginId}");
            var savedObject = await _syncBackendService.SaveGame(game, cancellationToken);
            if (savedObject.Id != game.Id.ToString())
            {
                Logger.Debug($"Game id mismatch: saved id = {savedObject.Id}, entity id = {game.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override async Task SaveDiffObject(object oldEntity, object newEntity,
            CancellationToken cancellationToken = default)
        {
            if (!(oldEntity is Game oldGame))
            {
                return;
            }

            if (!(newEntity is Game newGame))
            {
                return;
            }

            Logger.Trace(
                $"Saving game diff with id = {newGame.Id}, gameId = {newGame.GameId}, pluginId = {newGame.PluginId}");
            var savedObject = await _syncBackendService.SaveGameDiff(oldGame, newGame, cancellationToken);
            if (savedObject.Id != newGame.Id.ToString())
            {
                Logger.Debug($"Game id mismatch: saved id = {savedObject.Id}, entity id = {newGame.Id}");
                _dataProcessingService.ProcessIdMismatch(GetHandledType(), savedObject);
            }
        }

        public override Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is Game game))
            {
                return Task.CompletedTask;
            }

            Logger.Trace(
                $"Deleting game with id = {game.Id}, gameId = {game.GameId}, pluginId = {game.PluginId}");
            return _syncBackendService.DeleteGame(game, cancellationToken);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.Game;
        }

        protected override bool HasObjectChanged(ItemUpdateEvent<Game> args)
        {
            var newData = args.NewData;
            var oldData = args.OldData;
            return newData != null && (oldData == null || oldData.Id != newData.Id || oldData.Name != newData.Name ||
                                       oldData.Icon != newData.Icon || oldData.CoverImage != newData.CoverImage ||
                                       oldData.BackgroundImage != newData.BackgroundImage
                                       || (oldData.Description != newData.Description)
                                       || (oldData.Notes != newData.Notes)
                                       || (HaveItemsChanged(oldData.Genres, newData.Genres))
                                       || (oldData.Hidden != newData.Hidden)
                                       || (oldData.Favorite != newData.Favorite)
                                       || (oldData.LastActivity != newData.LastActivity)
                                       || (oldData.SortingName != newData.SortingName)
                                       || (oldData.GameId != newData.GameId)
                                       || (oldData.PluginId != newData.PluginId)
                                       || (HaveItemsChanged(oldData.Platforms, newData.Platforms))
                                       || (HaveItemsChanged(oldData.Publishers, newData.Publishers))
                                       || (HaveItemsChanged(oldData.Developers, newData.Developers))
                                       || (oldData.ReleaseDate != newData.ReleaseDate)
                                       || (HaveItemsChanged(oldData.Categories, newData.Categories))
                                       || (HaveItemsChanged(oldData.Tags, newData.Tags))
                                       || (HaveItemsChanged(oldData.Features, newData.Features))
                                       || (HaveLinksChanged(oldData.Links, newData.Links))
                                       || (oldData.Playtime != newData.Playtime)
                                       || (oldData.Added != newData.Added)
                                       || (oldData.Modified != newData.Modified)
                                       || (oldData.PlayCount != newData.PlayCount)
                                       || (oldData.InstallSize != newData.InstallSize)
                                       || (oldData.LastSizeScanDate != newData.LastSizeScanDate)
                                       || (HaveItemsChanged(oldData.Series, newData.Series))
                                       || (oldData.Version != newData.Version)
                                       || (HaveItemsChanged(oldData.AgeRatings, newData.AgeRatings))
                                       || (HaveItemsChanged(oldData.Regions, newData.Regions))
                                       || (oldData.SourceId != newData.SourceId)
                                       || (oldData.CompletionStatusId != newData.CompletionStatusId)
                                       || (oldData.UserScore != newData.UserScore)
                                       || (oldData.CriticScore != newData.CriticScore)
                                       || (oldData.CommunityScore != newData.CommunityScore)
                                       || (oldData.Manual != newData.Manual));
        }

        private static bool HaveItemsChanged<T>(List<T> oldItems, List<T> newItems) where T : DatabaseObject
        {
            var oldItemsCount = oldItems?.Count ?? 0;
            var newItemsCount = newItems?.Count ?? 0;
            return oldItemsCount != newItemsCount || (oldItems != null && newItems != null
                                                                       && !oldItems.All(t =>
                                                                           newItems.Any(i => i.Id == t.Id)));
        }

        private static bool HaveLinksChanged(Collection<Link> oldItems, Collection<Link> newItems)
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

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_Game");
        }
    }
}