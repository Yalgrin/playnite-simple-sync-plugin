using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class GameChangeRequestDto
    {
        [JsonProperty("ids")] public List<string> Ids { get; set; }
        [JsonProperty("gameIds")] public List<GameIdsDto> GameIds { get; set; }
    }

    public class GameIdsDto
    {
        [JsonProperty("gameId")] public string GameId { get; set; }
        [JsonProperty("pluginId")] public string PluginId { get; set; }
    }
}