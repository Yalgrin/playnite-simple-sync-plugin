using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class TagMapper : AbstractMapper<Tag, TagDto>
    {
        public TagDto ToDto(Tag entity)
        {
            return new TagDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name
            };
        }
    }
}