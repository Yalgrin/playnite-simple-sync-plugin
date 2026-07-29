using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class ErrorDto
    {
        [JsonProperty("message")] public string Message { get; set; }
    }
}