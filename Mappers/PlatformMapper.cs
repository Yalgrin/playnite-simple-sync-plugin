using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class PlatformMapper : AbstractMapper<Platform, PlatformDto>
    {
        public override void FillEntity(Platform entity, PlatformDto dto)
        {
            base.FillEntity(entity, dto);
            entity.SpecificationId = dto.SpecificationId;
        }

        public PlatformDto ToDto(Platform platform)
        {
            return new PlatformDto
            {
                Id = platform.Id.ToString(),
                Name = platform.Name,
                SpecificationId = platform.SpecificationId,
                HasIcon = platform.Icon != null,
                HasCoverImage = platform.Cover != null,
                HasBackgroundImage = platform.Background != null
            };
        }
    }
}