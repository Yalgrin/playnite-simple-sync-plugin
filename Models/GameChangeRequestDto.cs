using System.Collections.Generic;

namespace SimpleSyncPlugin.Models
{
    public class GameChangeRequestDto
    {
        public List<string> Ids { get; set; }
        public List<GameIdsDto> GameIds { get; set; }
    }

    public class GameIdsDto
    {
        public string GameId { get; set; }
        public string PluginId { get; set; }
    }
}