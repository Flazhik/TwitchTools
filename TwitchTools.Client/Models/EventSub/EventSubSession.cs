using Newtonsoft.Json;

namespace TwitchTools.Client.Models.EventSub;

internal sealed class EventSubSession
{
    [JsonProperty("id")]
    public string Id { get; set; }
}