using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public abstract class AbstractDiffSynchronizer<TEntity> : AbstractSynchronizer<TEntity>
        where TEntity : DatabaseObject
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        protected AbstractDiffSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService) : base(api,
            settingsViewModel, dataProcessingBroker, dataProcessingService)
        {
        }

        public override void RegisterListeners()
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

                // EnqueueChange(async () =>
                // {
                //     try
                //     {
                //         Logger.Debug($"Item collection changed, processing changes, sender: {sender}, args: {args}");
                //         foreach (var removedItem in removedItems)
                //         {
                //             if (IsNotInGracePeriod(removedItem.Id))
                //             {
                //                 await DeleteObject(removedItem);
                //             }
                //             else
                //             {
                //                 Logger.Trace($"Item {removedItem.Id} is in grace period, skipping...");
                //             }
                //         }
                //
                //         foreach (var addedItem in addedItems)
                //         {
                //             if (IsNotInGracePeriod(addedItem.Id))
                //             {
                //                 await SaveObject(addedItem);
                //             }
                //             else
                //             {
                //                 Logger.Trace($"Item {addedItem.Id} is in grace period, skipping...");
                //             }
                //         }
                //     }
                //     catch (Exception ex)
                //     {
                //         Logger.Error(ex, "Error occurred while saving new or removed objects!");
                //         _api.Notifications.Add(new NotificationMessage(ItemCollectionSaveErrorId,
                //             string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ItemCollectionSaveError"),
                //                 ex.Message), NotificationType.Error));
                //     }
                // });
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

                // EnqueueChange(async () =>
                // {
                //     try
                //     {
                //         Logger.Debug($"Item changed, processing changes, sender: {sender}, args: {args}");
                //         foreach (var updatedItem in args.UpdatedItems.Where(HasObjectChanged))
                //         {
                //             if (IsNotInGracePeriod(updatedItem.NewData.Id))
                //             {
                //                 if (updatedItem.OldData == null)
                //                 {
                //                     await SaveObject(updatedItem.NewData);
                //                 }
                //                 else
                //                 {
                //                     await SaveDiffObject(updatedItem.OldData, updatedItem.NewData);
                //                 }
                //             }
                //             else
                //             {
                //                 Logger.Trace($"Item {updatedItem.NewData.Id} is in grace period, skipping...");
                //             }
                //         }
                //     }
                //     catch (Exception ex)
                //     {
                //         Logger.Error(ex, "Error occurred while saving updated objects!");
                //         _api.Notifications.Add(new NotificationMessage(ItemUpdatedSaveErrorId,
                //             string.Format(GetLocalizedString("LOC_Yalgrin_SimpleSync_Error_ItemUpdateSaveError"),
                //                 ex.Message), NotificationType.Error));
                //     }
                // });
            };
        }
    }
}