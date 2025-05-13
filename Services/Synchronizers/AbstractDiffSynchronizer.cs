using System;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public abstract class AbstractDiffSynchronizer<TEntity> : AbstractSynchronizer<TEntity>
        where TEntity : DatabaseObject
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        protected AbstractDiffSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel) :
            base(api, settingsViewModel)
        {
        }

        public override void RegisterListeners()
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
                            if (updatedItem.OldData == null)
                            {
                                await SaveObject(updatedItem.NewData);
                            }
                            else
                            {
                                await SaveDiffObject(updatedItem.OldData, updatedItem.NewData);
                            }
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

        protected abstract Task SaveDiffObject(TEntity oldEntity, TEntity newEntity);
    }
}