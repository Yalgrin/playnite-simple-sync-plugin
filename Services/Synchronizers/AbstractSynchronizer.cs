using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public abstract class AbstractSynchronizer<TEntity> : IObjectSynchronizer where TEntity : DatabaseObject
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        protected const string ItemCollectionSaveErrorId = "Yalgrin-SimpleSyncPlugin-ItemCollectionSaveError";
        protected const string ItemUpdatedSaveErrorId = "Yalgrin-SimpleSyncPlugin-ItemUpdatedSaveError";

        protected readonly IPlayniteAPI _api;
        protected readonly SimpleSyncPluginSettingsViewModel _settingsViewModel;
        protected readonly DataProcessingBroker DataProcessingBroker;
        protected readonly DataProcessingService _dataProcessingService;

        private readonly Dictionary<Guid, DateTime> _gracePeriodMap = new Dictionary<Guid, DateTime>();

        protected AbstractSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService)
        {
            _api = api;
            _settingsViewModel = settingsViewModel;
            DataProcessingBroker = dataProcessingBroker;
            _dataProcessingService = dataProcessingService;
        }

        public virtual void RegisterListeners()
        {
            var databaseCollection = GetDatabaseCollection(_api.Database);
            databaseCollection.ItemCollectionChanged += (sender, args) =>
            {
                if (!_settingsViewModel.Settings.SendLiveChanges)
                {
                    return;
                }

                var removedItems = args.RemovedItems.ToList();
                var addedItems = args.AddedItems.ToList();

                var handledType = GetHandledType();
                DataProcessingBroker.SubmitObjectDeleteList(handledType, removedItems);
                DataProcessingBroker.SubmitObjectSaveList(handledType, addedItems);
            };

            databaseCollection.ItemUpdated += (sender, args) =>
            {
                if (!_settingsViewModel.Settings.SendLiveChanges)
                {
                    return;
                }

                var updatedItems = args.UpdatedItems.ToArray();
                var handledType = GetHandledType();
                foreach (var e in updatedItems)
                {
                    DataProcessingBroker.SubmitObjectSaveDiff(handledType, e.OldData, e.NewData);
                }
            };
        }

        public bool IsNotInGracePeriod(Guid id)
        {
            lock (_gracePeriodMap)
            {
                return !_gracePeriodMap.ContainsKey(id) || _gracePeriodMap[id] < DateTime.Now;
            }
        }

        public void RegisterGracePeriod(Guid id)
        {
            lock (_gracePeriodMap)
            {
                Logger.Trace($"Registering grace period for {id}...");
                _gracePeriodMap[id] = DateTime.Now.AddMilliseconds(500);
            }
        }

        protected abstract IItemCollection<TEntity> GetDatabaseCollection(IGameDatabaseAPI db);

        protected virtual List<TEntity> GetOrderedCollection(IGameDatabaseAPI db)
        {
            return GetDatabaseCollection(db).OrderBy(o => o.Name).ToList();
        }

        public abstract Task SaveObject(object entity, CancellationToken cancellationToken = default);

        public Task SaveObjectById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = GetDatabaseCollection(_api.Database).Get(id);
            return entity != null ? SaveObject(entity, cancellationToken) : Task.CompletedTask;
        }

        public virtual Task SaveDiffObject(object oldEntity, object newEntity,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public abstract Task DeleteObject(object entity, CancellationToken cancellationToken = default);

        protected virtual bool HasObjectChanged(ItemUpdateEvent<TEntity> args)
        {
            var newData = args.NewData;
            var oldData = args.OldData;
            return newData != null && (oldData == null || oldData.Id != newData.Id || oldData.Name != newData.Name);
        }

        public abstract ObjectType GetHandledType();

        public virtual int GetElementCount(IGameDatabaseAPI db)
        {
            return GetDatabaseCollection(db).Count;
        }

        public async Task SyncAll(IGameDatabaseAPI db, GlobalProgressActionArgs progArgs)
        {
            var list = GetOrderedCollection(db);
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
            var list = GetOrderedCollection(db);
            var listCount = ids.Count;
            var i = 1;
            foreach (var databaseObject in list)
            {
                if (progArgs.CancelToken.IsCancellationRequested)
                {
                    Logger.Info("SyncSelected > END, cancel requested...");
                    return;
                }

                if (ids.Contains(databaseObject.Id))
                {
                    progArgs.Text = GetLocalizedString("LOC_Yalgrin_SimpleSync_Dialogs_Sync") + "\n" +
                                    GetLocalizedObjectName() + " - " + i + "/" + listCount + " - " +
                                    databaseObject.Name;

                    await SaveObject(databaseObject, progArgs.CancelToken);
                    progArgs.CurrentProgressValue++;
                    i++;
                }
            }
        }

        protected string GetLocalizedString(string key)
        {
            return _api.GetLocalizedString(key);
        }

        protected abstract string GetLocalizedObjectName();
    }
}