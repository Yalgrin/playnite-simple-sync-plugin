using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class SourceMapper : AbstractMapper<GameSource, SourceDto>
    {
        public SourceDto ToDto(GameSource entity)
        {
            return new SourceDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name
            };
        }
    }
}