using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public abstract class AbstractDto
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("isRemoved")] public bool Removed { get; set; }
    }
}