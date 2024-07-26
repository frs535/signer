using System.Text.Json.Serialization;

namespace signer;

public class RequestData
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }
    [JsonPropertyName("data")]
    public string Data { get; set; }
}