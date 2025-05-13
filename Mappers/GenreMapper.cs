using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class GenreMapper : AbstractMapper<Genre, GenreDto>
    {
        public GenreDto ToDto(Genre genre)
        {
            return new GenreDto
            {
                Id = genre.Id.ToString(),
                Name = genre.Name
            };
        }
    }
}