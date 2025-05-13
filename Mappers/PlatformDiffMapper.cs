using System;
using System.Collections.Generic;
using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class PlatformDiffMapper : AbstractMapper<Platform, PlatformDiffDto>
    {
        public override void FillEntity(Platform entity, PlatformDiffDto dto)
        {
            entity.Id = new Guid(dto.Id);
            var changeFields = dto.ChangedFields;
            if (changeFields != null)
            {
                if (changeFields.Contains("Name"))
                {
                    entity.Name = dto.Name;
                }

                if (changeFields.Contains("SpecificationId"))
                {
                    entity.SpecificationId = dto.SpecificationId;
                }
            }
        }

        public PlatformDiffDto ToDiffDto(Platform oldPlatform, Platform newPlatform)
        {
            var diffDto = new PlatformDiffDto
            {
                Id = newPlatform.Id.ToString(),
                Name = newPlatform.Name,
                ChangedFields = new List<string>()
            };
            if (oldPlatform.Id != newPlatform.Id)
            {
                diffDto.ChangedFields.Add("Id");
            }

            if (oldPlatform.Name != newPlatform.Name)
            {
                diffDto.ChangedFields.Add("Name");
            }

            if (oldPlatform.SpecificationId != newPlatform.SpecificationId)
            {
                diffDto.SpecificationId = newPlatform.SpecificationId;
                diffDto.ChangedFields.Add("SpecificationId");
            }

            if (oldPlatform.Icon != newPlatform.Icon)
            {
                diffDto.ChangedFields.Add("Icon");
            }

            if (oldPlatform.Cover != newPlatform.Cover)
            {
                diffDto.ChangedFields.Add("CoverImage");
            }

            if (oldPlatform.Background != newPlatform.Background)
            {
                diffDto.ChangedFields.Add("BackgroundImage");
            }

            return diffDto;
        }
    }
}