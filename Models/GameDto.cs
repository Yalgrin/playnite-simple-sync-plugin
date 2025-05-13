using System;
using System.Collections.Generic;

namespace SimpleSyncPlugin.Models
{
    public class GameDto : AbstractDto
    {
        public string Description { get; set; }
        public string Notes { get; set; }
        public List<GenreDto> Genres { get; set; }
        public bool Hidden { get; set; }
        public bool Favorite { get; set; }
        public DateTime? LastActivity { get; set; }
        public string SortingName { get; set; }
        public string GameId { get; set; }
        public string PluginId { get; set; }
        public bool IncludeLibraryPluginAction { get; set; }
        public List<PlatformDto> Platforms { get; set; }
        public List<CompanyDto> Publishers { get; set; }
        public List<CompanyDto> Developers { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<CategoryDto> Categories { get; set; }
        public List<TagDto> Tags { get; set; }
        public List<FeatureDto> Features { get; set; }
        public List<LinkDto> Links { get; set; }
        public ulong Playtime { get; set; }
        public DateTime? Added { get; set; }
        public DateTime? Modified { get; set; }
        public ulong PlayCount { get; set; }
        public ulong? InstallSize { get; set; }
        public DateTime? LastSizeScanDate { get; set; }
        public List<SeriesDto> Series { get; set; }
        public string Version { get; set; }
        public List<AgeRatingDto> AgeRatings { get; set; }
        public List<RegionDto> Regions { get; set; }
        public SourceDto Source { get; set; }
        public CompletionStatusDto CompletionStatus { get; set; }
        public int? UserScore { get; set; }
        public int? CriticScore { get; set; }
        public int? CommunityScore { get; set; }
        public string Manual { get; set; }

        public bool HasIcon { get; set; }
        public bool HasCoverImage { get; set; }
        public bool HasBackgroundImage { get; set; }
    }

    public class GameDiffDto : AbstractDiffDto
    {
        public string Description { get; set; }
        public string Notes { get; set; }
        public List<GenreDto> Genres { get; set; }
        public bool Hidden { get; set; }
        public bool Favorite { get; set; }
        public DateTime? LastActivity { get; set; }
        public string SortingName { get; set; }
        public string GameId { get; set; }
        public string PluginId { get; set; }
        public bool IncludeLibraryPluginAction { get; set; }
        public List<PlatformDto> Platforms { get; set; }
        public List<CompanyDto> Publishers { get; set; }
        public List<CompanyDto> Developers { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<CategoryDto> Categories { get; set; }
        public List<TagDto> Tags { get; set; }
        public List<FeatureDto> Features { get; set; }
        public List<LinkDto> Links { get; set; }
        public ulong Playtime { get; set; }
        public ulong? PlaytimeDiff { get; set; }
        public DateTime? Added { get; set; }
        public DateTime? Modified { get; set; }
        public ulong PlayCount { get; set; }
        public ulong? PlayCountDiff { get; set; }
        public ulong? InstallSize { get; set; }
        public DateTime? LastSizeScanDate { get; set; }
        public List<SeriesDto> Series { get; set; }
        public string Version { get; set; }
        public List<AgeRatingDto> AgeRatings { get; set; }
        public List<RegionDto> Regions { get; set; }
        public SourceDto Source { get; set; }
        public CompletionStatusDto CompletionStatus { get; set; }
        public int? UserScore { get; set; }
        public int? CriticScore { get; set; }
        public int? CommunityScore { get; set; }
        public string Manual { get; set; }
    }

    public class LinkDto
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}