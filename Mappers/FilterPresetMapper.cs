using System;
using System.Collections.Generic;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class FilterPresetMapper : AbstractMapper<FilterPreset, FilterPresetDto>
    {
        public FilterPresetDto ToDto(FilterPreset entity)
        {
            return new FilterPresetDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                Settings = MapSettings(entity.Settings),
                SortingOrder = entity.SortingOrder?.ToString(),
                SortingOrderDirection = entity.SortingOrderDirection?.ToString(),
                GroupingOrder = entity.GroupingOrder?.ToString(),
                ShowInFullscreenQuickSelection = entity.ShowInFullscreeQuickSelection
            };
        }

        private FilterPresetSettingsDto MapSettings(FilterPresetSettings entitySettings)
        {
            return new FilterPresetSettingsDto
            {
                UseAndFilteringStyle = entitySettings.UseAndFilteringStyle,
                IsInstalled = entitySettings.IsInstalled,
                IsUnInstalled = entitySettings.IsUnInstalled,
                Hidden = entitySettings.Hidden,
                Favorite = entitySettings.Favorite,
                Name = entitySettings.Name,
                Version = entitySettings.Version,

                ReleaseYear = MapStringProperties(entitySettings.ReleaseYear),

                Genre = MapIdProperties(entitySettings.Genre),
                Platform = MapIdProperties(entitySettings.Platform),
                Publisher = MapIdProperties(entitySettings.Publisher),
                Developer = MapIdProperties(entitySettings.Developer),
                Category = MapIdProperties(entitySettings.Category),
                Tag = MapIdProperties(entitySettings.Tag),
                Series = MapIdProperties(entitySettings.Series),
                Region = MapIdProperties(entitySettings.Region),
                Source = MapIdProperties(entitySettings.Source),
                AgeRating = MapIdProperties(entitySettings.AgeRating),
                Library = MapIdProperties(entitySettings.Library),
                CompletionStatuses = MapIdProperties(entitySettings.CompletionStatuses),
                Feature = MapIdProperties(entitySettings.Feature),

                UserScore = MapIntProperties(entitySettings.UserScore),
                CriticScore = MapIntProperties(entitySettings.CriticScore),
                CommunityScore = MapIntProperties(entitySettings.CommunityScore),
                LastActivity = MapIntProperties(entitySettings.LastActivity),
                RecentActivity = MapIntProperties(entitySettings.RecentActivity),
                Added = MapIntProperties(entitySettings.Added),
                Modified = MapIntProperties(entitySettings.Modified),
                PlayTime = MapIntProperties(entitySettings.PlayTime),
                InstallSize = MapIntProperties(entitySettings.InstallSize)
            };
        }

        private StringItemPropertiesDto MapStringProperties(StringFilterItemProperties itemProperties)
        {
            if (itemProperties == null)
            {
                return null;
            }

            return new StringItemPropertiesDto
            {
                Values = MapList(itemProperties.Values)
            };
        }

        private IdItemPropertiesDto MapIdProperties(IdItemFilterItemProperties itemProperties)
        {
            if (itemProperties == null)
            {
                return null;
            }

            return new IdItemPropertiesDto
            {
                Ids = MapIds(itemProperties.Ids),
                Text = itemProperties.Text
            };
        }

        private IntItemPropertiesDto MapIntProperties(EnumFilterItemProperties itemProperties)
        {
            if (itemProperties == null)
            {
                return null;
            }

            return new IntItemPropertiesDto
            {
                Values = MapList(itemProperties.Values)
            };
        }

        public override void FillEntity(FilterPreset entity, FilterPresetDto dto)
        {
            base.FillEntity(entity, dto);

            var dtoSettings = dto.Settings;
            if (dtoSettings != null)
            {
                var entitySettings = new FilterPresetSettings
                {
                    UseAndFilteringStyle = dtoSettings.UseAndFilteringStyle,
                    IsInstalled = dtoSettings.IsInstalled,
                    IsUnInstalled = dtoSettings.IsUnInstalled,
                    Hidden = dtoSettings.Hidden,
                    Favorite = dtoSettings.Favorite,
                    Name = dtoSettings.Name,
                    Version = dtoSettings.Version,

                    ReleaseYear = MapStringProperties(dtoSettings.ReleaseYear),

                    Genre = MapIdProperties(dtoSettings.Genre),
                    Platform = MapIdProperties(dtoSettings.Platform),
                    Publisher = MapIdProperties(dtoSettings.Publisher),
                    Developer = MapIdProperties(dtoSettings.Developer),
                    Category = MapIdProperties(dtoSettings.Category),
                    Tag = MapIdProperties(dtoSettings.Tag),
                    Series = MapIdProperties(dtoSettings.Series),
                    Region = MapIdProperties(dtoSettings.Region),
                    Source = MapIdProperties(dtoSettings.Source),
                    AgeRating = MapIdProperties(dtoSettings.AgeRating),
                    Library = MapIdProperties(dtoSettings.Library),
                    CompletionStatuses = MapIdProperties(dtoSettings.CompletionStatuses),
                    Feature = MapIdProperties(dtoSettings.Feature),

                    UserScore = MapIntProperties(dtoSettings.UserScore),
                    CriticScore = MapIntProperties(dtoSettings.CriticScore),
                    CommunityScore = MapIntProperties(dtoSettings.CommunityScore),
                    LastActivity = MapIntProperties(dtoSettings.LastActivity),
                    RecentActivity = MapIntProperties(dtoSettings.RecentActivity),
                    Added = MapIntProperties(dtoSettings.Added),
                    Modified = MapIntProperties(dtoSettings.Modified),
                    PlayTime = MapIntProperties(dtoSettings.PlayTime),
                    InstallSize = MapIntProperties(dtoSettings.InstallSize)
                };


                entity.Settings = entitySettings;
            }
            else
            {
                entity.Settings = null;
            }

            if (dto.SortingOrder != null && Enum.TryParse(dto.SortingOrder, out SortOrder sortOrder))
            {
                entity.SortingOrder = sortOrder;
            }
            else
            {
                entity.SortingOrder = null;
            }

            if (dto.SortingOrderDirection != null &&
                Enum.TryParse(dto.SortingOrderDirection, out SortOrderDirection sortOrderDirection))
            {
                entity.SortingOrderDirection = sortOrderDirection;
            }
            else
            {
                entity.SortingOrderDirection = null;
            }

            if (dto.GroupingOrder != null && Enum.TryParse(dto.GroupingOrder, out GroupableField groupableField))
            {
                entity.GroupingOrder = groupableField;
            }
            else
            {
                entity.GroupingOrder = null;
            }

            entity.ShowInFullscreeQuickSelection = dto.ShowInFullscreenQuickSelection;
        }

        private StringFilterItemProperties MapStringProperties(StringItemPropertiesDto itemProperties)
        {
            if (itemProperties == null)
            {
                return null;
            }

            return new StringFilterItemProperties()
            {
                Values = MapList(itemProperties.Values)
            };
        }

        private IdItemFilterItemProperties MapIdProperties(IdItemPropertiesDto itemProperties)
        {
            if (itemProperties == null)
            {
                return null;
            }

            return new IdItemFilterItemProperties()
            {
                Ids = MapIds(itemProperties.Ids),
                Text = itemProperties.Text
            };
        }

        private EnumFilterItemProperties MapIntProperties(IntItemPropertiesDto itemProperties)
        {
            if (itemProperties == null)
            {
                return null;
            }

            return new EnumFilterItemProperties
            {
                Values = MapList(itemProperties.Values)
            };
        }

        private List<string> MapIds(List<Guid> ids)
        {
            if (ids == null)
            {
                return null;
            }

            var list = new List<string>();
            foreach (var id in ids)
            {
                list.Add(id.ToString());
            }

            list.Sort();
            return list;
        }

        private List<Guid> MapIds(List<string> ids)
        {
            if (ids == null)
            {
                return null;
            }

            var list = new List<Guid>();
            foreach (var id in ids)
            {
                list.Add(new Guid(id));
            }

            list.Sort();
            return list;
        }

        private List<string> MapList(List<string> ids)
        {
            if (ids == null)
            {
                return null;
            }

            var list = new List<string>(ids);
            list.Sort();
            return list;
        }

        private List<int> MapList(List<int> ids)
        {
            if (ids == null)
            {
                return null;
            }

            var list = new List<int>(ids);
            list.Sort();
            return list;
        }
    }
}