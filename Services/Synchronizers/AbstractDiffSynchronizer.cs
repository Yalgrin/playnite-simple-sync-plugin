using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Settings;
using SimpleSyncPlugin.Threading;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    //TODO remove?
    public abstract class AbstractDiffSynchronizer<TEntity> : AbstractSynchronizer<TEntity>
        where TEntity : DatabaseObject
    {
        protected AbstractDiffSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            DataProcessingBroker dataProcessingBroker, DataProcessingService dataProcessingService) : base(api,
            settingsViewModel, dataProcessingBroker, dataProcessingService)
        {
        }
    }
}