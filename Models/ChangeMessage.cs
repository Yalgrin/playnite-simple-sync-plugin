using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SimpleSyncPlugin.Models
{
    public class ChangeMessage
    {
        [JsonProperty("id")] public long? Id { get; set; }
        [JsonProperty("type")] public ObjectType Type { get; set; }
        [JsonProperty("clientId")] public string ClientId { get; set; }
        [JsonProperty("objectId")] public long ObjectId { get; set; }
        [JsonProperty("isForceFetch")] public bool ForceFetch { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ObjectType
    {
        [EnumMember(Value = "CATEGORY")] Category,

        [EnumMember(Value = "GENRE")] Genre,

        [EnumMember(Value = "PLATFORM")] Platform,

        [EnumMember(Value = "PLATFORM_DIFF")] PlatformDiff,

        [EnumMember(Value = "COMPANY")] Company,

        [EnumMember(Value = "FEATURE")] Feature,

        [EnumMember(Value = "TAG")] Tag,

        [EnumMember(Value = "SERIES")] Series,

        [EnumMember(Value = "AGE_RATING")] AgeRating,

        [EnumMember(Value = "REGION")] Region,

        [EnumMember(Value = "SOURCE")] Source,

        [EnumMember(Value = "COMPLETION_STATUS")]
        CompletionStatus,

        [EnumMember(Value = "FILTER_PRESET")] FilterPreset,

        [EnumMember(Value = "GAME")] Game,

        [EnumMember(Value = "GAME_DIFF")] GameDiff
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