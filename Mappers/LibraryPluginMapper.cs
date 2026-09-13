using Playnite.SDK.Plugins;
using SimpleSyncPlugin.Models;

namespace SimpleSyncPlugin.Mappers
{
    public class LibraryPluginMapper
    {
        public LibraryPluginDto ToDto(LibraryPlugin plugin)
        {
            return new LibraryPluginDto
            {
                Id = plugin.Id.ToString(),
                Name = plugin.Name,
                HasIcon = plugin.LibraryIcon != null,
                HasBackgroundImage = plugin.LibraryBackground != null
            };
        }
    }
}