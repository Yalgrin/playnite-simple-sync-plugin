using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SimpleSyncPlugin.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CheckResult
    {
        [EnumMember(Value = "OK")] Ok,

        [EnumMember(Value = "OUTDATED_CLIENT")]
        OutdatedClient,

        [EnumMember(Value = "OUTDATED_SERVER")]
        OutdatedServer
    }

    public class CheckRequestDto
    {
        [JsonProperty("supportedApiVersion")] public int SupportedApiVersion { get; set; }
    }

    public class InitializationMessage
    {
        [JsonProperty("sessionId")] public string SessionId { get; set; }
    }

    public class RegistrationRequestDto
    {
        [JsonProperty("displayName")] public string DisplayName { get; set; }

        [JsonProperty("supportedApiVersion")] public int SupportedApiVersion { get; set; }
    }

    public class CheckResultDto
    {
        [JsonProperty("result")] public CheckResult Result { get; set; }
    }

    public class RegisteredClientDto
    {
        [JsonProperty("clientId")] public string ClientId { get; set; }

        [JsonProperty("displayName")] public string DisplayName { get; set; }

        [JsonProperty("clientToken")] public string ClientToken { get; set; }

        public override string ToString()
        {
            return $"RegisteredClientDto(DisplayName='{DisplayName}')";
        }
    }
}