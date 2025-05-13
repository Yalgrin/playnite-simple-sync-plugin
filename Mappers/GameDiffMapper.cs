using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class GameDiffMapper : AbstractMapper<Game, GameDiffDto>
    {
        private readonly GenreMapper _genreMapper = new GenreMapper();
        private readonly PlatformMapper _platformMapper = new PlatformMapper();
        private readonly CompanyMapper _companyMapper = new CompanyMapper();
        private readonly CategoryMapper _categoryMapper = new CategoryMapper();
        private readonly TagMapper _tagMapper = new TagMapper();
        private readonly FeatureMapper _featureMapper = new FeatureMapper();
        private readonly SeriesMapper _seriesMapper = new SeriesMapper();
        private readonly AgeRatingMapper _ageRatingMapper = new AgeRatingMapper();
        private readonly RegionMapper _regionMapper = new RegionMapper();
        private readonly SourceMapper _sourceMapper = new SourceMapper();
        private readonly CompletionStatusMapper _completionStatusMapper = new CompletionStatusMapper();

        private readonly IPlayniteAPI _api;

        public GameDiffMapper(IPlayniteAPI api)
        {
            _api = api;
        }

        public override void FillEntity(Game entity, GameDiffDto dto)
        {
            entity.Id = new Guid(dto.Id);
            entity.GameId = dto.GameId;
            entity.PluginId = new Guid(dto.PluginId);
            var changeFields = dto.ChangedFields;
            if (changeFields != null)
            {
                if (changeFields.Contains("Name"))
                {
                    entity.Name = dto.Name;
                }

                if (changeFields.Contains("Description"))
                {
                    entity.Description = dto.Description;
                }

                if (changeFields.Contains("Notes"))
                {
                    entity.Notes = dto.Notes;
                }

                if (changeFields.Contains("Genres"))
                {
                    if (entity.GenreIds == null)
                    {
                        entity.GenreIds = new List<Guid>();
                    }

                    entity.GenreIds.Clear();
                    dto.Genres?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.GenreIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Hidden"))
                {
                    entity.Hidden = dto.Hidden;
                }

                if (changeFields.Contains("Favorite"))
                {
                    entity.Favorite = dto.Favorite;
                }

                if (changeFields.Contains("LastActivity"))
                {
                    entity.LastActivity = dto.LastActivity;
                }

                if (changeFields.Contains("SortingName"))
                {
                    entity.SortingName = dto.SortingName;
                }

                if (changeFields.Contains("GameId") && entity.GameId == null)
                {
                    entity.GameId = dto.GameId;
                }

                if (changeFields.Contains("PluginId") && entity.PluginId != Guid.Empty)
                {
                    entity.PluginId = new Guid(dto.PluginId);
                }

                if (changeFields.Contains("IncludeLibraryPluginAction"))
                {
                    entity.IncludeLibraryPluginAction = dto.IncludeLibraryPluginAction;
                }

                if (changeFields.Contains("Platforms"))
                {
                    if (entity.PlatformIds == null)
                    {
                        entity.PlatformIds = new List<Guid>();
                    }

                    entity.PlatformIds.Clear();
                    dto.Platforms?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.PlatformIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Publishers"))
                {
                    if (entity.PublisherIds == null)
                    {
                        entity.PublisherIds = new List<Guid>();
                    }

                    entity.PublisherIds.Clear();
                    dto.Publishers?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.PublisherIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Developers"))
                {
                    if (entity.DeveloperIds == null)
                    {
                        entity.DeveloperIds = new List<Guid>();
                    }

                    entity.DeveloperIds.Clear();
                    dto.Developers?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.DeveloperIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("ReleaseDate"))
                {
                    if (dto.ReleaseDate != null)
                    {
                        entity.ReleaseDate = new ReleaseDate((DateTime)dto.ReleaseDate);
                    }
                    else
                    {
                        entity.ReleaseDate = null;
                    }
                }

                if (changeFields.Contains("Categories"))
                {
                    if (entity.CategoryIds == null)
                    {
                        entity.CategoryIds = new List<Guid>();
                    }

                    entity.CategoryIds.Clear();
                    dto.Categories?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.CategoryIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Tags"))
                {
                    if (entity.TagIds == null)
                    {
                        entity.TagIds = new List<Guid>();
                    }

                    entity.TagIds.Clear();
                    dto.Tags?.Select(d => d.Id).Where(id => id != null).ForEach(id => entity.TagIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Features"))
                {
                    if (entity.FeatureIds == null)
                    {
                        entity.FeatureIds = new List<Guid>();
                    }

                    entity.FeatureIds.Clear();
                    dto.Features?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.FeatureIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Links"))
                {
                    _api.MainView.UIDispatcher.Invoke(delegate
                    {
                        if (entity.Links == null)
                        {
                            entity.Links = new ObservableCollection<Link>();
                        }

                        entity.Links.Clear();
                        if (dto.Links != null)
                        {
                            foreach (var link in dto.Links)
                            {
                                entity.Links.Add(new Link(link.Name, link.Url));
                            }
                        }
                    });
                }

                if (changeFields.Contains("Playtime"))
                {
                    entity.Playtime = dto.Playtime;
                }

                if (dto.Added != null && changeFields.Contains("Added"))
                {
                    entity.Added = dto.Added;
                }

                if (dto.Modified != null && changeFields.Contains("Modified"))
                {
                    entity.Modified = dto.Modified;
                }

                if (changeFields.Contains("PlayCount"))
                {
                    entity.PlayCount = dto.PlayCount;
                }

                if (changeFields.Contains("InstallSize"))
                {
                    entity.InstallSize = dto.InstallSize;
                }

                if (changeFields.Contains("LastSizeScanDate"))
                {
                    entity.LastSizeScanDate = dto.LastSizeScanDate;
                }

                if (changeFields.Contains("Series"))
                {
                    if (entity.SeriesIds == null)
                    {
                        entity.SeriesIds = new List<Guid>();
                    }

                    entity.SeriesIds.Clear();
                    dto.Series?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.SeriesIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Version"))
                {
                    entity.Version = dto.Version;
                }

                if (changeFields.Contains("AgeRatings"))
                {
                    if (entity.AgeRatingIds == null)
                    {
                        entity.AgeRatingIds = new List<Guid>();
                    }

                    entity.AgeRatingIds.Clear();
                    dto.AgeRatings?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.AgeRatingIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Regions"))
                {
                    if (entity.RegionIds == null)
                    {
                        entity.RegionIds = new List<Guid>();
                    }

                    entity.RegionIds.Clear();
                    dto.Regions?.Select(d => d.Id).Where(id => id != null)
                        .ForEach(id => entity.RegionIds.Add(new Guid(id)));
                }

                if (changeFields.Contains("Source"))
                {
                    entity.SourceId = dto.Source?.Id != null ? new Guid(dto.Source.Id) : Guid.Empty;
                }

                if (changeFields.Contains("CompletionStatus"))
                {
                    entity.CompletionStatusId =
                        dto.CompletionStatus?.Id != null ? new Guid(dto.CompletionStatus.Id) : Guid.Empty;
                }

                if (changeFields.Contains("UserScore"))
                {
                    entity.UserScore = dto.UserScore;
                }

                if (changeFields.Contains("CriticScore"))
                {
                    entity.CriticScore = dto.CriticScore;
                }

                if (changeFields.Contains("CommunityScore"))
                {
                    entity.CommunityScore = dto.CommunityScore;
                }

                if (changeFields.Contains("Manual"))
                {
                    entity.Manual = dto.Manual;
                }
            }
        }

        public GameDiffDto ToDiffDto(Game oldGame, Game newGame)
        {
            var diffDto = new GameDiffDto
            {
                Id = newGame.Id.ToString(),
                Name = newGame.Name,
                GameId = newGame.GameId,
                PluginId = newGame.PluginId.ToString(),
                ChangedFields = new List<string>()
            };
            if (oldGame.Id != newGame.Id)
            {
                diffDto.ChangedFields.Add("Id");
            }

            if (oldGame.Name != newGame.Name)
            {
                diffDto.ChangedFields.Add("Name");
            }

            if (oldGame.GameId != newGame.GameId)
            {
                diffDto.ChangedFields.Add("GameId");
            }

            if (oldGame.PluginId != newGame.PluginId)
            {
                diffDto.ChangedFields.Add("PluginId");
            }

            if (oldGame.Description != newGame.Description)
            {
                diffDto.Description = newGame.Description;
                diffDto.ChangedFields.Add("Description");
            }

            if (oldGame.Notes != newGame.Notes)
            {
                diffDto.Notes = newGame.Notes;
                diffDto.ChangedFields.Add("Notes");
            }

            if (HaveItemsChanged(oldGame.Genres, newGame.Genres))
            {
                diffDto.Genres = newGame.Genres?.Select(g => _genreMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Genres");
            }

            if (oldGame.Hidden != newGame.Hidden)
            {
                diffDto.Hidden = newGame.Hidden;
                diffDto.ChangedFields.Add("Hidden");
            }

            if (oldGame.Favorite != newGame.Favorite)
            {
                diffDto.Favorite = newGame.Favorite;
                diffDto.ChangedFields.Add("Favorite");
            }

            if (oldGame.LastActivity != newGame.LastActivity)
            {
                diffDto.LastActivity = newGame.LastActivity;
                diffDto.ChangedFields.Add("LastActivity");
            }

            if (oldGame.SortingName != newGame.SortingName)
            {
                diffDto.SortingName = newGame.SortingName;
                diffDto.ChangedFields.Add("SortingName");
            }

            if (oldGame.GameId != newGame.GameId)
            {
                diffDto.GameId = newGame.GameId;
                diffDto.ChangedFields.Add("GameId");
            }

            if (oldGame.PluginId != newGame.PluginId)
            {
                diffDto.PluginId = newGame.PluginId.ToString();
                diffDto.ChangedFields.Add("PluginId");
            }

            if (oldGame.IncludeLibraryPluginAction != newGame.IncludeLibraryPluginAction)
            {
                diffDto.IncludeLibraryPluginAction = newGame.IncludeLibraryPluginAction;
                diffDto.ChangedFields.Add("IncludeLibraryPluginAction");
            }

            if (HaveItemsChanged(oldGame.Platforms, newGame.Platforms))
            {
                diffDto.Platforms = newGame.Platforms?.Select(g => _platformMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Platforms");
            }

            if (HaveItemsChanged(oldGame.Publishers, newGame.Publishers))
            {
                diffDto.Publishers = newGame.Publishers?.Select(g => _companyMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Publishers");
            }

            if (HaveItemsChanged(oldGame.Developers, newGame.Developers))
            {
                diffDto.Developers = newGame.Developers?.Select(g => _companyMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Developers");
            }

            if (oldGame.ReleaseDate != newGame.ReleaseDate)
            {
                diffDto.ReleaseDate = newGame.ReleaseDate?.Date;
                diffDto.ChangedFields.Add("ReleaseDate");
            }

            if (HaveItemsChanged(oldGame.Categories, newGame.Categories))
            {
                diffDto.Categories = newGame.Categories?.Select(g => _categoryMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Categories");
            }

            if (HaveItemsChanged(oldGame.Tags, newGame.Tags))
            {
                diffDto.Tags = newGame.Tags?.Select(g => _tagMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Tags");
            }

            if (HaveItemsChanged(oldGame.Features, newGame.Features))
            {
                diffDto.Features = newGame.Features?.Select(g => _featureMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Features");
            }

            if (HaveLinksChanged(oldGame.Links, newGame.Links))
            {
                diffDto.Links = newGame.Links?.Select(g => new LinkDto
                {
                    Name = g.Name,
                    Url = g.Url
                }).ToList();
                diffDto.ChangedFields.Add("Links");
            }

            if (oldGame.Playtime != newGame.Playtime)
            {
                diffDto.Playtime = newGame.Playtime;
                if (!newGame.IsRunning && !newGame.IsLaunching && (oldGame.IsRunning || newGame.IsLaunching))
                {
                    diffDto.PlaytimeDiff = newGame.Playtime - oldGame.Playtime;
                }

                diffDto.ChangedFields.Add("Playtime");
            }

            if (oldGame.Added != newGame.Added)
            {
                diffDto.Added = newGame.Added;
                diffDto.ChangedFields.Add("Added");
            }

            if (oldGame.Modified != newGame.Modified)
            {
                diffDto.Modified = newGame.Modified;
                diffDto.ChangedFields.Add("Modified");
            }

            if (oldGame.PlayCount != newGame.PlayCount)
            {
                diffDto.PlayCount = newGame.PlayCount;
                if (newGame.IsRunning && !oldGame.IsRunning)
                {
                    diffDto.PlayCountDiff = newGame.PlayCount - oldGame.PlayCount;
                }

                diffDto.ChangedFields.Add("PlayCount");
            }

            if (oldGame.InstallSize != newGame.InstallSize)
            {
                diffDto.InstallSize = newGame.InstallSize;
                diffDto.ChangedFields.Add("InstallSize");
            }

            if (oldGame.LastSizeScanDate != newGame.LastSizeScanDate)
            {
                diffDto.LastSizeScanDate = newGame.LastSizeScanDate;
                diffDto.ChangedFields.Add("LastSizeScanDate");
            }

            if (HaveItemsChanged(oldGame.Series, newGame.Series))
            {
                diffDto.Series = newGame.Series?.Select(g => _seriesMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Series");
            }

            if (oldGame.Version != newGame.Version)
            {
                diffDto.Version = newGame.Version;
                diffDto.ChangedFields.Add("Version");
            }

            if (HaveItemsChanged(oldGame.AgeRatings, newGame.AgeRatings))
            {
                diffDto.AgeRatings = newGame.AgeRatings?.Select(g => _ageRatingMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("AgeRatings");
            }

            if (HaveItemsChanged(oldGame.Regions, newGame.Regions))
            {
                diffDto.Regions = newGame.Regions?.Select(g => _regionMapper.ToDto(g)).ToList();
                diffDto.ChangedFields.Add("Regions");
            }

            if (oldGame.Source?.Id != newGame.Source?.Id)
            {
                diffDto.Source = newGame.Source != null ? _sourceMapper.ToDto(newGame.Source) : null;
                diffDto.ChangedFields.Add("Source");
            }

            if (oldGame.CompletionStatus?.Id != newGame.CompletionStatus?.Id)
            {
                diffDto.CompletionStatus = newGame.CompletionStatus != null
                    ? _completionStatusMapper.ToDto(newGame.CompletionStatus)
                    : null;
                diffDto.ChangedFields.Add("CompletionStatus");
            }

            if (oldGame.UserScore != newGame.UserScore)
            {
                diffDto.UserScore = newGame.UserScore;
                diffDto.ChangedFields.Add("UserScore");
            }

            if (oldGame.CriticScore != newGame.CriticScore)
            {
                diffDto.CriticScore = newGame.CriticScore;
                diffDto.ChangedFields.Add("CriticScore");
            }

            if (oldGame.CommunityScore != newGame.CommunityScore)
            {
                diffDto.CommunityScore = newGame.CommunityScore;
                diffDto.ChangedFields.Add("CommunityScore");
            }

            if (oldGame.Manual != newGame.Manual)
            {
                diffDto.Manual = newGame.Manual;
                diffDto.ChangedFields.Add("Manual");
            }

            if (oldGame.Icon != newGame.Icon)
            {
                diffDto.ChangedFields.Add("Icon");
            }

            if (oldGame.CoverImage != newGame.CoverImage)
            {
                diffDto.ChangedFields.Add("CoverImage");
            }

            if (oldGame.BackgroundImage != newGame.BackgroundImage)
            {
                diffDto.ChangedFields.Add("BackgroundImage");
            }

            return diffDto;
        }

        private static bool HaveItemsChanged<T>(List<T> oldItems, List<T> newItems) where T : DatabaseObject
        {
            var oldItemsCount = oldItems?.Count ?? 0;
            var newItemsCount = newItems?.Count ?? 0;
            return oldItemsCount != newItemsCount || (oldItems != null && newItems != null
                                                                       && !oldItems.All(t =>
                                                                           newItems.Any(i => i.Id == t.Id)));
        }

        private static bool HaveLinksChanged(Collection<Link> oldItems, Collection<Link> newItems)
        {
            var oldItemsCount = oldItems?.Count ?? 0;
            var newItemsCount = newItems?.Count ?? 0;
            if (oldItemsCount != newItemsCount)
            {
                return true;
            }

            if (oldItems != null && newItems != null)
            {
                for (var i = 0; i < oldItems.Count; i++)
                {
                    var oldLink = oldItems[i];
                    var newLink = newItems[i];
                    if (oldLink.Name != newLink.Name || oldLink.Url != newLink.Url)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}