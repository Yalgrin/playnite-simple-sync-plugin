using System.Collections.Generic;

namespace SimpleSyncPlugin.Models
{
    public class FilterPresetDto : AbstractDto
    {
        public FilterPresetSettingsDto Settings { get; set; }
        public string SortingOrder { get; set; }
        public string SortingOrderDirection { get; set; }
        public string GroupingOrder { get; set; }
        public bool ShowInFullscreenQuickSelection { get; set; }
    }

    public class FilterPresetSettingsDto
    {
        public bool UseAndFilteringStyle { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsUnInstalled { get; set; }
        public bool Hidden { get; set; }
        public bool Favorite { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }

        public StringItemPropertiesDto ReleaseYear { get; set; }

        public IdItemPropertiesDto Genre { get; set; }
        public IdItemPropertiesDto Platform { get; set; }
        public IdItemPropertiesDto Publisher { get; set; }
        public IdItemPropertiesDto Developer { get; set; }
        public IdItemPropertiesDto Category { get; set; }
        public IdItemPropertiesDto Tag { get; set; }
        public IdItemPropertiesDto Series { get; set; }
        public IdItemPropertiesDto Region { get; set; }
        public IdItemPropertiesDto Source { get; set; }
        public IdItemPropertiesDto AgeRating { get; set; }
        public IdItemPropertiesDto Library { get; set; }
        public IdItemPropertiesDto CompletionStatuses { get; set; }
        public IdItemPropertiesDto Feature { get; set; }

        public IntItemPropertiesDto UserScore { get; set; }
        public IntItemPropertiesDto CriticScore { get; set; }
        public IntItemPropertiesDto CommunityScore { get; set; }
        public IntItemPropertiesDto LastActivity { get; set; }
        public IntItemPropertiesDto RecentActivity { get; set; }
        public IntItemPropertiesDto Added { get; set; }
        public IntItemPropertiesDto Modified { get; set; }
        public IntItemPropertiesDto PlayTime { get; set; }
        public IntItemPropertiesDto InstallSize { get; set; }
    }

    public class StringItemPropertiesDto
    {
        public List<string> Values { get; set; }
    }

    public class IdItemPropertiesDto
    {
        public List<string> Ids { get; set; }
        public string Text { get; set; }
    }

    public class IntItemPropertiesDto
    {
        public List<int> Values { get; set; }
    }
}