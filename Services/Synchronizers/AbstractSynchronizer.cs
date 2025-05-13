using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Extensions;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public abstract class AbstractSynchronizer<TEntity> : IObjectSynchronizer where TEntity : DatabaseObject
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        protected const string ItemCollectionSaveErrorId = "Yalgrin-SimpleSyncPlugin-ItemCollectionSaveError";
        protected const string ItemUpdatedSaveErrorId = "Yalgrin-SimpleSyncPlugin-ItemUpdatedSaveError";

        protected readonly IPlayniteAPI _api;
        protected readonly SimpleSyncPluginSettingsViewModel _settingsViewModel;

        private readonly Dictionary<Guid, DateTime> _gracePeriodMap = new Dictionary<Guid, DateTime>();

        protected AbstractSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel)
        {
            _api = api;
            _settingsViewModel = settingsViewModel;
        }

        public virtual void RegisterListeners()
        {
            var databaseCollection = GetDatabaseCollection(_api.Database);
            databaseCollection.ItemCollectionChanged += async (sender, args) =>
            {
                if (!_settingsViewModel.Settings.SendLiveChanges)
                {
                    return;
                }

                try
                {
                    foreach (var removedItem in args.RemovedItems)
                    {
                        if (IsNotInGracePeriod(removedItem.Id))
                        {
                            await DeleteObject(removedItem);
                        }
                        else
                        {
                            Logger.Trace($"Item {removedItem.Id} is in grace period, skipping...");
                        }
                    }

                    foreach (var addedItem in args.AddedItems)
                    {
                        if (IsNotInGracePeriod(addedItem.Id))
                        {
                            await SaveObject(addedItem);
                        }
                        else
                        {
                            Logger.Trace($"Item {addedItem.Id} is in grace period, skipping...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error occurred while saving new or removed objects!");
                    _api.Notifications.Add(new NotificationMessage(ItemCollectionSaveErrorId,
                        string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ItemCollectionSaveError"),
                            ex.Message), NotificationType.Error));
                }
            };

            databaseCollection.ItemUpdated += async (sender, args) =>
            {
                if (!_settingsViewModel.Settings.SendLiveChanges)
                {
                    return;
                }

                try
                {
                    foreach (var updatedItem in args.UpdatedItems.Where(HasObjectChanged))
                    {
                        if (IsNotInGracePeriod(updatedItem.NewData.Id))
                        {
                            await SaveObject(updatedItem.NewData);
                        }
                        else
                        {
                            Logger.Trace($"Item {updatedItem.NewData.Id} is in grace period, skipping...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error occurred while saving updated objects!");
                    _api.Notifications.Add(new NotificationMessage(ItemUpdatedSaveErrorId,
                        string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ItemUpdateSaveError"),
                            ex.Message), NotificationType.Error));
                }
            };
        }

        protected bool IsNotInGracePeriod(Guid id)
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

        protected abstract Task SaveObject(TEntity entity);

        protected abstract Task DeleteObject(TEntity entity);

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

                await SaveObject(databaseObject);
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

                    await SaveObject(databaseObject);
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