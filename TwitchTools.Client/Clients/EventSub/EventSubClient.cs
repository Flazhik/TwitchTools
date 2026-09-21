using System;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwitchTools.Client.Environment;
using TwitchTools.Client.Models.EventSub;
using TwitchTools.Client.Utils;
using UnityEngine.Networking;
using WebSocketSharp;

namespace TwitchTools.Client.Clients.EventSub;

internal sealed class EventSubClient(TwitchEnvironment environment) : IDisposable
{
    public event Action<bool> Connected;
    public event Action<RedeemEvent> OnRedeem;

    private TwitchSession _session;
    private WebSocket _socket;
    private string _sessionId;

    public Action<string> Logger { get; set; }

    public void Connect(TwitchSession session)
    {
        if (_socket?.IsAlive == true)
        {
            Log("Already listening to EventSub");
            return;
        }

        _session = session;
        _sessionId = null;

        _socket = new WebSocket(environment.EventSubWsUrl);
        _socket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
        _socket.SslConfiguration.CheckCertificateRevocation = false;

        _socket.OnOpen += OnOpen;
        _socket.OnMessage += OnMessage;
        _socket.OnError += OnError;
        _socket.OnClose += OnClose;

        Log("Connecting to EventSub");
        _socket.ConnectAsync();
    }

    public void Disconnect()
    {
        if (_socket == null)
            return;

        var socket = _socket;
        _socket = null;

        socket.OnOpen -= OnOpen;
        socket.OnMessage -= OnMessage;
        socket.OnError -= OnError;
        socket.OnClose -= OnClose;

        socket.Close();
        Connected?.Invoke(false);
    }

    private void OnOpen(object sender, EventArgs e) => Log("Connected to EventSub");

    private void OnClose(object sender, CloseEventArgs e)
    {
        Log("Connection closed: " + e.Reason);
        Connected?.Invoke(false);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Log("WebSocket error: " + e.Message);

        if (_socket?.IsAlive != true)
            Connected?.Invoke(false);
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        try
        {
            HandleMessage(e.Data);
        }
        catch (Exception ex)
        {
            Log($"Couldn't handle EventSub message: {ex}");
            Log(e.Data);
        }
    }

    private void HandleMessage(string json)
    {
        var msg = JsonConvert.DeserializeObject<EventSubMessage>(json);
        if (msg == null)
        {
            Log("Failed to deserialize EventSub message");
            return;
        }

        switch (msg.Metadata.MessageType)
        {
            case "session_welcome":
                HandleWelcome(msg);
                break;

            case "notification":
                HandleNotification(msg);
                break;

            case "session_keepalive":
                break;

            default:
                Log("Unknown EventSub message: " + msg.Metadata.MessageType);
                break;
        }
    }

    private void HandleWelcome(EventSubMessage message) =>
        _ = HandleWelcomeAsync(message);

    private async Task HandleWelcomeAsync(EventSubMessage message)
    {
        _sessionId = message.Payload.Session.Id;
        try
        {
            await SubscribeToRedeemsAsync();
            Connected?.Invoke(true);
        }
        catch (Exception e)
        {
            Log($"Couldn't subscribe to redeems: {e}");
            Disconnect();
        }
    }

    private void HandleNotification(EventSubMessage message)
    {
        if (message.Metadata.SubscriptionType != "channel.channel_points_custom_reward_redemption.add")
            return;

        var redeem = message.Payload.Event.ToObject<RedeemEvent>();
        if (redeem == null)
        {
            Log("Failed to deserialize redeem event");
            return;
        }

        OnRedeem?.Invoke(redeem);
    }

    private async Task SubscribeToRedeemsAsync()
    {
        if (string.IsNullOrEmpty(_sessionId))
            throw new InvalidOperationException("EventSub session is not ready");

        var body = new
        {
            type = "channel.channel_points_custom_reward_redemption.add",
            version = "1",
            condition = new { broadcaster_user_id = _session.UserId },
            transport = new
            {
                method = "websocket",
                session_id = _sessionId
            }
        };

        var json = JsonConvert.SerializeObject(body);

        using var request = new UnityWebRequest(environment.HelixUrl + "/eventsub/subscriptions", "POST");
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Authorization", $"Bearer {_session.AccessToken}");
        request.SetRequestHeader("Client-Id", _session.ClientId);
        request.SetRequestHeader("Content-Type", "application/json");
        await request.SendAsync();

        Log("Redeems subscription created");
    }

    private void Log(string message) => Logger?.Invoke("[EventSub] " + message);

    public void Dispose()
    {
        Disconnect();
        _session = null;
        _sessionId = null;
    }
}