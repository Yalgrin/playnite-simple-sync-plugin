using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;
using SimpleSyncPlugin.Settings;

namespace SimpleSyncPlugin.Services.Synchronizers
{
    public class FilterPresetSynchronizer : AbstractSynchronizer<FilterPreset>
    {
        private static readonly ILogger Logger = LogManager.GetLogger();

        private readonly SyncBackendService _syncBackendService;

        public FilterPresetSynchronizer(IPlayniteAPI api, SimpleSyncPluginSettingsViewModel settingsViewModel,
            SyncBackendService syncBackendService) : base(api, settingsViewModel)
        {
            _syncBackendService = syncBackendService;
        }

        protected override IItemCollection<FilterPreset> GetDatabaseCollection(IGameDatabaseAPI db)
        {
            return db.FilterPresets;
        }

        protected override bool HasObjectChanged(ItemUpdateEvent<FilterPreset> args)
        {
            var newData = args.NewData;
            var oldData = args.OldData;
            return newData != null && (oldData == null || oldData.Id != newData.Id || oldData.Name != newData.Name ||
                                       HasSettingsChanged(oldData.Settings, newData.Settings)
                                       || oldData.SortingOrder != newData.SortingOrder
                                       || oldData.SortingOrderDirection != newData.SortingOrderDirection
                                       || oldData.GroupingOrder != newData.GroupingOrder
                                       || oldData.ShowInFullscreeQuickSelection !=
                                       newData.ShowInFullscreeQuickSelection);
        }

        private bool HasSettingsChanged(FilterPresetSettings oldData, FilterPresetSettings newData)
        {
            return ((oldData == null) != (newData == null))
                   || (oldData != null && (
                       oldData.UseAndFilteringStyle != newData.UseAndFilteringStyle
                       || oldData.IsInstalled != newData.IsInstalled
                       || oldData.IsUnInstalled != newData.IsUnInstalled
                       || oldData.Hidden != newData.Hidden
                       || oldData.Favorite != newData.Favorite
                       || oldData.Name != newData.Name
                       || oldData.Version != newData.Version
                       || HasItemPropertiesChanged(oldData.ReleaseYear, newData.ReleaseYear)
                       || HasItemPropertiesChanged(oldData.Genre, newData.Genre)
                       || HasItemPropertiesChanged(oldData.Platform, newData.Platform)
                       || HasItemPropertiesChanged(oldData.Publisher, newData.Publisher)
                       || HasItemPropertiesChanged(oldData.Developer, newData.Developer)
                       || HasItemPropertiesChanged(oldData.Category, newData.Category)
                       || HasItemPropertiesChanged(oldData.Tag, newData.Tag)
                       || HasItemPropertiesChanged(oldData.Series, newData.Series)
                       || HasItemPropertiesChanged(oldData.Region, newData.Region)
                       || HasItemPropertiesChanged(oldData.Source, newData.Source)
                       || HasItemPropertiesChanged(oldData.AgeRating, newData.AgeRating)
                       || HasItemPropertiesChanged(oldData.Library, newData.Library)
                       || HasItemPropertiesChanged(oldData.CompletionStatuses, newData.CompletionStatuses)
                       || HasItemPropertiesChanged(oldData.Feature, newData.Feature)
                       || HasItemPropertiesChanged(oldData.UserScore, newData.UserScore)
                       || HasItemPropertiesChanged(oldData.CriticScore, newData.CriticScore)
                       || HasItemPropertiesChanged(oldData.CommunityScore, newData.CommunityScore)
                       || HasItemPropertiesChanged(oldData.LastActivity, newData.LastActivity)
                       || HasItemPropertiesChanged(oldData.RecentActivity, newData.RecentActivity)
                       || HasItemPropertiesChanged(oldData.Added, newData.Added)
                       || HasItemPropertiesChanged(oldData.Modified, newData.Modified)
                       || HasItemPropertiesChanged(oldData.PlayTime, newData.PlayTime)
                       || HasItemPropertiesChanged(oldData.InstallSize, newData.InstallSize)
                   ));
        }

        private bool HasItemPropertiesChanged(StringFilterItemProperties oldData, StringFilterItemProperties newData)
        {
            return ((oldData == null) != (newData == null))
                   || (oldData != null && HasListChanged(oldData.Values, newData.Values));
        }

        private bool HasItemPropertiesChanged(IdItemFilterItemProperties oldData, IdItemFilterItemProperties newData)
        {
            return ((oldData == null) != (newData == null))
                   || (oldData != null && (HasListChanged(oldData.Ids, newData.Ids) || oldData.Text != newData.Text));
        }

        private bool HasItemPropertiesChanged(EnumFilterItemProperties oldData, EnumFilterItemProperties newData)
        {
            return ((oldData == null) != (newData == null))
                   || (oldData != null && HasListChanged(oldData.Values, newData.Values));
        }

        private bool HasListChanged<T>(List<T> oldDataValues, List<T> newDataValues)
        {
            var oldCount = oldDataValues?.Count ?? 0;
            var newCount = newDataValues?.Count ?? 0;
            return oldCount != newCount || (oldDataValues != null && newDataValues != null &&
                                            oldDataValues.Any(newDataValues.Contains));
        }

        protected override Task SaveObject(FilterPreset entity)
        {
            Logger.Trace($"Saving filter preset with id = {entity.Id}");
            return _syncBackendService.SaveFilterPreset(entity);
        }

        protected override Task DeleteObject(FilterPreset entity)
        {
            Logger.Trace($"Deleting filter preset with id = {entity.Id}");
            return _syncBackendService.DeleteFilterPreset(entity);
        }

        public override ObjectType GetHandledType()
        {
            return ObjectType.FilterPreset;
        }

        protected override string GetLocalizedObjectName()
        {
            return GetLocalizedString("LOC_Yalgrin_SimpleSync_ObjectType_FilterPreset");
        }
    }
}