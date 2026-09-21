using Newtonsoft.Json;

namespace TwitchTools.Client.Models.EventSub;

public sealed class RewardInfo
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
        
    [JsonProperty("cost")]
    public int Cost { get; set; }
}