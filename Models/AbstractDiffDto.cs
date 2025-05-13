using System.Collections.Generic;

namespace SimpleSyncPlugin.Models
{
    public abstract class AbstractDiffDto : AbstractDto
    {
        public long BaseObjectId { get; set; }
        public List<string> ChangedFields { get; set; }
    }
}