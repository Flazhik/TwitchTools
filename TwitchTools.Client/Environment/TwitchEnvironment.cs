using System;
using TwitchTools.Client.Models.EventSub;

namespace TwitchTools.Client.Environment;

public sealed class TwitchEnvironment
{
    public string EventSubWsUrl { get; }
    public string HelixUrl { get; }
    public Func<RedeemEvent, string> GetPromptFrom { get; }

    private TwitchEnvironment(string eventSubWsUrl, string helixUrl, Func<RedeemEvent, string> getPromptFrom)
    {
        EventSubWsUrl = eventSubWsUrl;
        HelixUrl = helixUrl;
        GetPromptFrom = getPromptFrom;
    }

    public static TwitchEnvironment Production => new(
        "wss://eventsub.wss.twitch.tv/ws", 
        "https://api.twitch.tv/helix",
        static redeem => redeem.UserInput);

    public static TwitchEnvironment Sandbox => new(
        "ws://127.0.0.1:8080/ws",
        "http://127.0.0.1:8080",
        static redeem => redeem.Reward.Id);
}