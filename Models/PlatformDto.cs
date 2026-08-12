using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class PlatformDto : AbstractDto
    {
        [JsonProperty("specificationId")] public string SpecificationId { get; set; }

        [JsonProperty("hasIcon")] public bool HasIcon { get; set; }

        [JsonProperty("hasCoverImage")] public bool HasCoverImage { get; set; }

        [JsonProperty("hasBackgroundImage")] public bool HasBackgroundImage { get; set; }
    }

    public class PlatformDiffDto : AbstractDiffDto
    {
        [JsonProperty("specificationId")] public string SpecificationId { get; set; }
    }
}