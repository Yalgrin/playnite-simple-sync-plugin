using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class SeriesMapper : AbstractMapper<Series, SeriesDto>
    {
        public SeriesDto ToDto(Series entity)
        {
            return new SeriesDto
            {
                Id = entity.Id.ToString(),
                Name = entity.Name
            };
        }
    }
}