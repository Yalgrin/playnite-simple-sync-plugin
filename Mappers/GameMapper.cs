using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class GameMapper : AbstractMapper<Game, GameDto>
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

        public GameMapper(IPlayniteAPI api)
        {
            _api = api;
        }

        public override void FillEntity(Game entity, GameDto dto)
        {
            base.FillEntity(entity, dto);
            entity.Description = dto.Description;
            entity.Notes = dto.Notes;

            if (entity.GenreIds == null)
            {
                entity.GenreIds = new List<Guid>();
            }

            entity.GenreIds.Clear();
            dto.Genres?.Select(d => d.Id).Where(id => id != null).ForEach(id => entity.GenreIds.Add(new Guid(id)));

            entity.Hidden = dto.Hidden;
            entity.Favorite = dto.Favorite;
            entity.LastActivity = dto.LastActivity;
            entity.SortingName = dto.SortingName;
            entity.GameId = dto.GameId;
            entity.PluginId = new Guid(dto.PluginId);

            if (entity.PlatformIds == null)
            {
                entity.PlatformIds = new List<Guid>();
            }

            entity.PlatformIds.Clear();
            dto.Platforms?.Select(d => d.Id).Where(id => id != null)
                .ForEach(id => entity.PlatformIds.Add(new Guid(id)));
            if (entity.PublisherIds == null)
            {
                entity.PublisherIds = new List<Guid>();
            }

            entity.PublisherIds.Clear();
            dto.Publishers?.Select(d => d.Id).Where(id => id != null)
                .ForEach(id => entity.PublisherIds.Add(new Guid(id)));
            if (entity.DeveloperIds == null)
            {
                entity.DeveloperIds = new List<Guid>();
            }

            entity.DeveloperIds.Clear();
            dto.Developers?.Select(d => d.Id).Where(id => id != null)
                .ForEach(id => entity.DeveloperIds.Add(new Guid(id)));

            if (dto.ReleaseDate != null)
            {
                entity.ReleaseDate = new ReleaseDate((DateTime)dto.ReleaseDate);
            }
            else
            {
                entity.ReleaseDate = null;
            }

            if (entity.CategoryIds == null)
            {
                entity.CategoryIds = new List<Guid>();
            }

            entity.CategoryIds.Clear();
            dto.Categories?.Select(d => d.Id).Where(id => id != null)
                .ForEach(id => entity.CategoryIds.Add(new Guid(id)));
            if (entity.TagIds == null)
            {
                entity.TagIds = new List<Guid>();
            }

            entity.TagIds.Clear();
            dto.Tags?.Select(d => d.Id).Where(id => id != null).ForEach(id => entity.TagIds.Add(new Guid(id)));
            if (entity.FeatureIds == null)
            {
                entity.FeatureIds = new List<Guid>();
            }

            entity.FeatureIds.Clear();
            dto.Features?.Select(d => d.Id).Where(id => id != null).ForEach(id => entity.FeatureIds.Add(new Guid(id)));

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

            entity.Playtime = dto.Playtime;
            if (dto.Added != null)
            {
                entity.Added = dto.Added;
            }

            if (dto.Modified != null)
            {
                entity.Modified = dto.Modified;
            }

            entity.PlayCount = dto.PlayCount;
            entity.InstallSize = dto.InstallSize;
            entity.LastSizeScanDate = dto.LastSizeScanDate;

            if (entity.SeriesIds == null)
            {
                entity.SeriesIds = new List<Guid>();
            }

            entity.SeriesIds.Clear();
            dto.Series?.Select(d => d.Id).Where(id => id != null).ForEach(id => entity.SeriesIds.Add(new Guid(id)));

            entity.Version = dto.Version;

            if (entity.AgeRatingIds == null)
            {
                entity.AgeRatingIds = new List<Guid>();
            }

            entity.AgeRatingIds.Clear();
            dto.AgeRatings?.Select(d => d.Id).Where(id => id != null)
                .ForEach(id => entity.AgeRatingIds.Add(new Guid(id)));
            if (entity.RegionIds == null)
            {
                entity.RegionIds = new List<Guid>();
            }

            entity.RegionIds.Clear();
            dto.Regions?.Select(d => d.Id).Where(id => id != null).ForEach(id => entity.RegionIds.Add(new Guid(id)));
            entity.SourceId = dto.Source?.Id != null ? new Guid(dto.Source.Id) : Guid.Empty;
            entity.CompletionStatusId =
                dto.CompletionStatus?.Id != null ? new Guid(dto.CompletionStatus.Id) : Guid.Empty;

            entity.UserScore = dto.UserScore;
            entity.CriticScore = dto.CriticScore;
            entity.CommunityScore = dto.CommunityScore;
            entity.Manual = dto.Manual;
        }

        public GameDto ToDto(Game entity)
        {
            return new GameDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                Description = entity.Description,
                Notes = entity.Notes,
                Genres = entity.Genres?.Select(g => _genreMapper.ToDto(g)).ToList(),
                Hidden = entity.Hidden,
                Favorite = entity.Favorite,
                LastActivity = entity.LastActivity,
                SortingName = entity.SortingName,
                GameId = entity.GameId,
                PluginId = entity.PluginId.ToString(),
                Platforms = entity.Platforms?.Select(g => _platformMapper.ToDto(g)).ToList(),
                Publishers = entity.Publishers?.Select(g => _companyMapper.ToDto(g)).ToList(),
                Developers = entity.Developers?.Select(g => _companyMapper.ToDto(g)).ToList(),
                ReleaseDate = entity.ReleaseDate?.Date,
                Categories = entity.Categories?.Select(g => _categoryMapper.ToDto(g)).ToList(),
                Tags = entity.Tags?.Select(g => _tagMapper.ToDto(g)).ToList(),
                Features = entity.Features?.Select(g => _featureMapper.ToDto(g)).ToList(),
                Links = entity.Links?.Select(g => new LinkDto
                {
                    Name = g.Name,
                    Url = g.Url
                }).ToList(),
                Playtime = entity.Playtime,
                Added = entity.Added,
                Modified = entity.Modified,
                PlayCount = entity.PlayCount,
                InstallSize = entity.InstallSize,
                LastSizeScanDate = entity.LastSizeScanDate,
                Series = entity.Series?.Select(g => _seriesMapper.ToDto(g)).ToList(),
                Version = entity.Version,
                AgeRatings = entity.AgeRatings?.Select(g => _ageRatingMapper.ToDto(g)).ToList(),
                Regions = entity.Regions?.Select(g => _regionMapper.ToDto(g)).ToList(),
                Source = entity.Source != null ? _sourceMapper.ToDto(entity.Source) : null,
                CompletionStatus = entity.CompletionStatus != null
                    ? _completionStatusMapper.ToDto(entity.CompletionStatus)
                    : null,
                UserScore = entity.UserScore,
                CriticScore = entity.CriticScore,
                CommunityScore = entity.CommunityScore,
                Manual = entity.Manual,
                HasIcon = entity.Icon != null,
                HasCoverImage = entity.CoverImage != null,
                HasBackgroundImage = entity.BackgroundImage != null
            };
        }
    }
}