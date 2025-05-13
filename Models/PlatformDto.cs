namespace SimpleSyncPlugin.Models
{
    public class PlatformDto : AbstractDto
    {
        public string SpecificationId { get; set; }

        public bool HasIcon { get; set; }

        public bool HasCoverImage { get; set; }

        public bool HasBackgroundImage { get; set; }
    }

    public class PlatformDiffDto : AbstractDiffDto
    {
        public string SpecificationId { get; set; }
    }
}