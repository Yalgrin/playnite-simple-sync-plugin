namespace SimpleSyncPlugin.Models
{
    public abstract class AbstractDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Removed { get; set; }
    }
}