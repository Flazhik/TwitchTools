using Newtonsoft.Json;

namespace TwitchTools.Client.Models.OAuth;

internal sealed class OAuthErrorResponse
{
    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }
}