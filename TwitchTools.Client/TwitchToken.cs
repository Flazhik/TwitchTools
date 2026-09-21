using System;
using Newtonsoft.Json;
using TwitchTools.Client.Models.OAuth;

namespace TwitchTools.Client;

internal sealed class TwitchToken
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
        
    [JsonIgnore]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        
    public static TwitchToken From(TokenResponse response)
    {
        return new TwitchToken
        {
            AccessToken = response.AccessToken,
            RefreshToken = response.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddSeconds(response.ExpiresIn)
        };
    }
}