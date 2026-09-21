using Newtonsoft.Json;

namespace TwitchTools.Client.Models.EventSub;

public sealed class RedeemEvent
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("broadcaster_user_id")]
    public string BroadcasterUserId { get; set; }

    [JsonProperty("user_id")]
    public string UserId { get; set; }

    [JsonProperty("user_login")]
    public string UserLogin { get; set; }
        
    [JsonProperty("user_name")]
    public string UserName { get; set; }

    [JsonProperty("user_input")]
    public string UserInput { get; set; }

    [JsonProperty("reward")]
    public RewardInfo Reward { get; set; }
}