using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class FeatureMapper : AbstractMapper<GameFeature, FeatureDto>
    {
        public FeatureDto ToDto(GameFeature Feature)
        {
            return new FeatureDto
            {
                Id = Feature.Id.ToString(),
                Name = Feature.Name
            };
        }
    }
}