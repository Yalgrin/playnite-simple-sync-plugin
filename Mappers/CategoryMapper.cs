using Playnite.SDK.Models;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class CategoryMapper : AbstractMapper<Category, CategoryDto>
    {
        public CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id.ToString(),
                Name = category.Name
            };
        }
    }
}