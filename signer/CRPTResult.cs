using System.Text.Json.Serialization;

namespace signer;

public class CRPTResult
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}