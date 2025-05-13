using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class RegionMapper : AbstractMapper<Region, RegionDto>
    {
        public RegionDto ToDto(Region entity)
        {
            return new RegionDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                SpecificationId = entity.SpecificationId
            };
        }

        public override void FillEntity(Region entity, RegionDto dto)
        {
            base.FillEntity(entity, dto);
            entity.SpecificationId = dto.SpecificationId;
        }
    }
}