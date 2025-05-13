using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class AgeRatingMapper : AbstractMapper<AgeRating, AgeRatingDto>
    {
        public AgeRatingDto ToDto(AgeRating entity)
        {
            return new AgeRatingDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name
            };
        }
    }
}