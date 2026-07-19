using Newtonsoft.Json;

namespace SimpleSyncPlugin.Models
{
    public class ChangeMessage
    {
        [JsonProperty("id")] public long? Id { get; set; }
        [JsonProperty("type")] public ObjectType Type { get; set; }
        [JsonProperty("clientId")] public string ClientId { get; set; }
        [JsonProperty("objectId")] public long ObjectId { get; set; }
        [JsonProperty("forceFetch")] public bool ForceFetch { get; set; }
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