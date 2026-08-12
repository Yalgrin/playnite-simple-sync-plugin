using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class RegionDto : AbstractDto
    {
        [JsonProperty("specificationId")] public string SpecificationId { get; set; }
    }
}