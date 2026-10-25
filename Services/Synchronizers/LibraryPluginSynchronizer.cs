using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using SimpleSyncPlugin.Commons;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public sealed class LibraryPluginSynchronizer : IObjectSynchronizer
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly IPlayniteAPI _api;
        private readonly SyncBackendService _syncBackendService;
        private readonly DataProcessingBroker _dataProcessingBroker;
        private readonly DataProcessingService _dataProcessingService;

        public LibraryPluginSynchronizer(IPlayniteAPI api, SyncBackendService syncBackendService,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService)
        {
            _api = api;
            _syncBackendService = syncBackendService;
            _dataProcessingBroker = dataProcessingBroker;
            _dataProcessingService = dataProcessingService;
        }

        public void RegisterListeners()
        {
            //unsupported, will only sync when requested
        }

        public ObjectType GetHandledType() => ObjectType.LibraryPlugin;

        public int GetElementCount(IGameDatabaseAPI db)
        {
            return _api.Addons.Plugins.Count(x => x is LibraryPlugin) + 1;
        }

        public async Task SyncAll(IGameDatabaseAPI db, GlobalProgressActionArgs progArgs)
        {
            var list = GetAllLibraryPlugins();
            var listCount = list.Count;
            var i = 1;
            Logger.Info($"SyncAll > going to sync {listCount} objects of type {GetHandledType()}");
            foreach (var databaseObject in list)
            {
                if (progArgs.CancelToken.IsCancellationRequested)
                {
                    Logger.Info("SyncAll > END, cancel requested...");
                    return;
                }

                progArgs.Text = GetLocalizedString("LOC_Yalgrin_SimpleSync_Dialogs_Sync") + "\n" +
                                GetLocalizedObjectName() + " - " + i + "/" + listCount + " - " + databaseObject.Name;

                await SaveObject(databaseObject, progArgs.CancelToken);
                progArgs.CurrentProgressValue++;
                i++;
            }
        }

        public async Task SyncSelected(IGameDatabaseAPI db, GlobalProgressActionArgs progArgs, List<Guid> ids)
        {
            var list = GetAllLibraryPlugins();
            var listCount = ids.Count;
            var i = 1;
            foreach (var databaseObject in list)
            {
                if (progArgs.CancelToken.IsCancellationRequested)
                {
                    Logger.Info("SyncSelected > END, cancel requested...");
                    return;
                }

                if (!ids.Contains(databaseObject.Id))
                {
                    continue;
                }

                progArgs.Text = GetLocalizedString("LOC_Yalgrin_SimpleSync_Dialogs_Sync") + "\n" +
                                GetLocalizedObjectName() + " - " + i + "/" + listCount + " - " +
                                databaseObject.Name;

                await SaveObject(databaseObject, progArgs.CancelToken);
                progArgs.CurrentProgressValue++;
                i++;
            }
        }

        private List<LibraryPlugin> GetAllLibraryPlugins()
        {
            var result = new List<LibraryPlugin> { new FakePlayniteLibraryPlugin() };
            foreach (var plugin in _api.Addons.Plugins)
            {
                if (plugin is LibraryPlugin libraryPlugin)
                {
                    result.Add(libraryPlugin);
                }
            }

            return result;
        }

        public void RegisterGracePeriod(Guid id)
        {
            //unsupported, will only sync when requested
        }

        public Task SaveObject(object entity, CancellationToken cancellationToken = default)
        {
            if (!(entity is LibraryPlugin libraryPlugin))
            {
                return Task.CompletedTask;
            }

            Logger.Trace($"Saving library plugin with id = {libraryPlugin.Id}");
            return _syncBackendService.SaveLibraryPlugin(libraryPlugin, cancellationToken);
        }

        public Task SaveObjectById(Guid id, CancellationToken cancellationToken = default)
        {
            var allLibraryPlugins = GetAllLibraryPlugins();
            var entity = allLibraryPlugins.Find(e => e.Id == id);
            return entity != null ? SaveObject(entity, cancellationToken) : Task.CompletedTask;
        }

        public Task SaveDiffObject(object oldEntity, object newEntity, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task DeleteObject(object entity, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private string GetLocalizedString(string key)
        {
            return _api.GetLocalizedString(key);
        }

        private string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_LibraryPlugin");
        }
    }

    [IgnorePlugin]
    internal class FakePlayniteLibraryPlugin : LibraryPlugin
    {
        public override string Name => Constants.PlaynitePluginName;

        public override Guid Id => Guid.Empty;

        public FakePlayniteLibraryPlugin() : base(null)
        {
        }

        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            throw new NotImplementedException();
        }
    }
}