using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class LibraryPluginDto : AbstractDto
    {
        [JsonProperty("specificationId")] public string SpecificationId { get; set; }

        [JsonProperty("hasIcon")] public bool HasIcon { get; set; }

        [JsonProperty("hasBackgroundImage")] public bool HasBackgroundImage { get; set; }
    }

    public class LibraryPluginDiffDto : AbstractDiffDto
    {
    }
}