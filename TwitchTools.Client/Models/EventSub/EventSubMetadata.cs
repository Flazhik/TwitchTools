using Newtonsoft.Json;

namespace TwitchTools.Client.Models.EventSub;

internal sealed class EventSubMetadata
{
    [JsonProperty("message_type")]
    public string MessageType { get; set; }
        
    [JsonProperty("subscription_type")]
    public string SubscriptionType { get; set; }
}