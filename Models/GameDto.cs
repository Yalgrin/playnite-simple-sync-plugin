using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class GameDto : AbstractDto
    {
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("notes")] public string Notes { get; set; }
        [JsonProperty("genres")] public List<GenreDto> Genres { get; set; }
        [JsonProperty("isHidden")] public bool Hidden { get; set; }
        [JsonProperty("isFavorite")] public bool Favorite { get; set; }
        [JsonProperty("lastActivity")] public DateTime? LastActivity { get; set; }
        [JsonProperty("sortingName")] public string SortingName { get; set; }
        [JsonProperty("gameId")] public string GameId { get; set; }
        [JsonProperty("pluginId")] public string PluginId { get; set; }
        [JsonProperty("platforms")] public List<PlatformDto> Platforms { get; set; }
        [JsonProperty("publishers")] public List<CompanyDto> Publishers { get; set; }
        [JsonProperty("developers")] public List<CompanyDto> Developers { get; set; }
        [JsonProperty("releaseDate")] public DateTime? ReleaseDate { get; set; }
        [JsonProperty("categories")] public List<CategoryDto> Categories { get; set; }
        [JsonProperty("tags")] public List<TagDto> Tags { get; set; }
        [JsonProperty("features")] public List<FeatureDto> Features { get; set; }
        [JsonProperty("links")] public List<LinkDto> Links { get; set; }
        [JsonProperty("playtime")] public ulong Playtime { get; set; }
        [JsonProperty("added")] public DateTime? Added { get; set; }
        [JsonProperty("modified")] public DateTime? Modified { get; set; }
        [JsonProperty("playCount")] public ulong PlayCount { get; set; }
        [JsonProperty("installSize")] public ulong? InstallSize { get; set; }
        [JsonProperty("lastSizeScanDate")] public DateTime? LastSizeScanDate { get; set; }
        [JsonProperty("series")] public List<SeriesDto> Series { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("ageRatings")] public List<AgeRatingDto> AgeRatings { get; set; }
        [JsonProperty("regions")] public List<RegionDto> Regions { get; set; }
        [JsonProperty("source")] public SourceDto Source { get; set; }
        [JsonProperty("completionStatus")] public CompletionStatusDto CompletionStatus { get; set; }
        [JsonProperty("userScore")] public int? UserScore { get; set; }
        [JsonProperty("criticScore")] public int? CriticScore { get; set; }
        [JsonProperty("communityScore")] public int? CommunityScore { get; set; }
        [JsonProperty("manual")] public string Manual { get; set; }

        [JsonProperty("hasIcon")] public bool HasIcon { get; set; }
        [JsonProperty("hasCoverImage")] public bool HasCoverImage { get; set; }
        [JsonProperty("hasBackgroundImage")] public bool HasBackgroundImage { get; set; }
    }

    public class GameDiffDto : AbstractDiffDto
    {
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("notes")] public string Notes { get; set; }
        [JsonProperty("genres")] public List<GenreDto> Genres { get; set; }
        [JsonProperty("isHidden")] public bool Hidden { get; set; }
        [JsonProperty("isFavorite")] public bool Favorite { get; set; }
        [JsonProperty("lastActivity")] public DateTime? LastActivity { get; set; }
        [JsonProperty("sortingName")] public string SortingName { get; set; }
        [JsonProperty("gameId")] public string GameId { get; set; }
        [JsonProperty("pluginId")] public string PluginId { get; set; }
        [JsonProperty("platforms")] public List<PlatformDto> Platforms { get; set; }
        [JsonProperty("publishers")] public List<CompanyDto> Publishers { get; set; }
        [JsonProperty("developers")] public List<CompanyDto> Developers { get; set; }
        [JsonProperty("releaseDate")] public DateTime? ReleaseDate { get; set; }
        [JsonProperty("categories")] public List<CategoryDto> Categories { get; set; }
        [JsonProperty("tags")] public List<TagDto> Tags { get; set; }
        [JsonProperty("features")] public List<FeatureDto> Features { get; set; }
        [JsonProperty("links")] public List<LinkDto> Links { get; set; }
        [JsonProperty("playtime")] public ulong Playtime { get; set; }
        [JsonProperty("playtimeDiff")] public ulong? PlaytimeDiff { get; set; }
        [JsonProperty("added")] public DateTime? Added { get; set; }
        [JsonProperty("modified")] public DateTime? Modified { get; set; }
        [JsonProperty("playCount")] public ulong PlayCount { get; set; }
        [JsonProperty("playCountDiff")] public ulong? PlayCountDiff { get; set; }
        [JsonProperty("installSize")] public ulong? InstallSize { get; set; }
        [JsonProperty("lastSizeScanDate")] public DateTime? LastSizeScanDate { get; set; }
        [JsonProperty("series")] public List<SeriesDto> Series { get; set; }
        [JsonProperty("version")] public string Version { get; set; }
        [JsonProperty("ageRatings")] public List<AgeRatingDto> AgeRatings { get; set; }
        [JsonProperty("regions")] public List<RegionDto> Regions { get; set; }
        [JsonProperty("source")] public SourceDto Source { get; set; }
        [JsonProperty("completionStatus")] public CompletionStatusDto CompletionStatus { get; set; }
        [JsonProperty("userScore")] public int? UserScore { get; set; }
        [JsonProperty("criticScore")] public int? CriticScore { get; set; }
        [JsonProperty("communityScore")] public int? CommunityScore { get; set; }
        [JsonProperty("manual")] public string Manual { get; set; }
    }

    public class LinkDto
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("url")] public string Url { get; set; }
    }
}