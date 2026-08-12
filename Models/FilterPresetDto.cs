using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class FilterPresetDto : AbstractDto
    {
        [JsonProperty("settings")] public FilterPresetSettingsDto Settings { get; set; }
        [JsonProperty("sortingOrder")] public string SortingOrder { get; set; }

        [JsonProperty("sortingOrderDirection")]
        public string SortingOrderDirection { get; set; }

        [JsonProperty("groupingOrder")] public string GroupingOrder { get; set; }

        [JsonProperty("showInFullscreenQuickSelection")]
        public bool ShowInFullscreenQuickSelection { get; set; }
    }

    public class FilterPresetSettingsDto
    {
        [JsonProperty("useAndFilteringStyle")] public bool UseAndFilteringStyle { get; set; }
        [JsonProperty("isInstalled")] public bool IsInstalled { get; set; }
        [JsonProperty("isUninstalled")] public bool IsUnInstalled { get; set; }
        [JsonProperty("isHidden")] public bool Hidden { get; set; }
        [JsonProperty("isFavorite")] public bool Favorite { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("releaseYear")] public StringItemPropertiesDto ReleaseYear { get; set; }

        [JsonProperty("genre")] public IdItemPropertiesDto Genre { get; set; }
        [JsonProperty("platform")] public IdItemPropertiesDto Platform { get; set; }
        [JsonProperty("publisher")] public IdItemPropertiesDto Publisher { get; set; }
        [JsonProperty("developer")] public IdItemPropertiesDto Developer { get; set; }
        [JsonProperty("category")] public IdItemPropertiesDto Category { get; set; }
        [JsonProperty("tag")] public IdItemPropertiesDto Tag { get; set; }
        [JsonProperty("series")] public IdItemPropertiesDto Series { get; set; }
        [JsonProperty("region")] public IdItemPropertiesDto Region { get; set; }
        [JsonProperty("source")] public IdItemPropertiesDto Source { get; set; }
        [JsonProperty("ageRating")] public IdItemPropertiesDto AgeRating { get; set; }
        [JsonProperty("library")] public IdItemPropertiesDto Library { get; set; }
        [JsonProperty("completionStatuses")] public IdItemPropertiesDto CompletionStatuses { get; set; }
        [JsonProperty("feature")] public IdItemPropertiesDto Feature { get; set; }

        [JsonProperty("userScore")] public IntItemPropertiesDto UserScore { get; set; }
        [JsonProperty("criticScore")] public IntItemPropertiesDto CriticScore { get; set; }
        [JsonProperty("communityScore")] public IntItemPropertiesDto CommunityScore { get; set; }
        [JsonProperty("lastActivity")] public IntItemPropertiesDto LastActivity { get; set; }
        [JsonProperty("recentActivity")] public IntItemPropertiesDto RecentActivity { get; set; }
        [JsonProperty("added")] public IntItemPropertiesDto Added { get; set; }
        [JsonProperty("modified")] public IntItemPropertiesDto Modified { get; set; }
        [JsonProperty("playTime")] public IntItemPropertiesDto PlayTime { get; set; }
        [JsonProperty("installSize")] public IntItemPropertiesDto InstallSize { get; set; }
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