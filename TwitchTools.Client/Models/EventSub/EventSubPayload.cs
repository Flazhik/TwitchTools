using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TwitchTools.Client.Models.EventSub;

internal sealed class EventSubPayload
{
    [JsonProperty("session")]
    public EventSubSession Session { get; set; }
        
    [JsonProperty("event")]
    public JObject Event { get; set; }
}