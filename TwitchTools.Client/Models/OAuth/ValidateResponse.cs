using Newtonsoft.Json;

namespace TwitchTools.Client.Models.OAuth;

internal sealed class ValidateResponse
{
    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("login")]
    public string Login { get; set; }

    [JsonProperty("user_id")]
    public string UserId { get; set; }

    [JsonProperty("scopes")]
    public string[] Scopes { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
}