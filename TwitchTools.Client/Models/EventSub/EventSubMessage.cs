using Newtonsoft.Json;

namespace TwitchTools.Client.Models.EventSub;

internal sealed class EventSubMessage
{
    [JsonProperty("metadata")]
    public EventSubMetadata Metadata { get; set; }

    [JsonProperty("payload")]
    public EventSubPayload Payload { get; set; }
}