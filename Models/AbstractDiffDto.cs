using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public abstract class AbstractDiffDto : AbstractDto
    {
        [JsonProperty("baseObjectId")] public long BaseObjectId { get; set; }

        [JsonProperty("changedFields")] public List<string> ChangedFields { get; set; }
    }
}