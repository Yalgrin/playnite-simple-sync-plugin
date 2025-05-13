namespace SimpleSyncPlugin.Models
{
    public class ChangeDto
    {
        public long? Id { get; set; }
        public ObjectType Type { get; set; }
        public string ClientId { get; set; }
        public long ObjectId { get; set; }
        public bool ForceFetch { get; set; }
    }

    public enum ObjectType
    {
        Category,
        Genre,
        Platform,
        PlatformDiff,
        Company,
        Feature,
        Tag,
        Series,
        AgeRating,
        Region,
        Source,
        CompletionStatus,
        FilterPreset,
        Game,
        GameDiff
    }

    public static class ObjectTypeExtension
    {
        public static ObjectType GetBaseObjectType(this ObjectType type)
        {
            switch (type)
            {
                case ObjectType.PlatformDiff:
                    return ObjectType.Platform;
                case ObjectType.GameDiff:
                    return ObjectType.Game;
                default:
                    return type;
            }
        }
    }
}