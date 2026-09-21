using System;
using System.Threading.Tasks;
using TwitchTools.Client.Clients.EventSub;
using TwitchTools.Client.Clients.OAuth;
using TwitchTools.Client.Environment;
using TwitchTools.Client.Models.EventSub;

namespace TwitchTools.Client.Clients;

public sealed class TwitchClient : IDisposable
{
    public event Action<State> OnStateChanged;
    public event Action<RedeemEvent> OnRedeem;
        
    private readonly TwitchOAuthClient _twitchOAuthClient;
    private readonly EventSubClient _eventSubClient;
    private readonly Action<string> _logger;

    public TwitchClient(string clientId, string tokensPath, Action<string> logger, TwitchEnvironment environment)
    {
        _logger = logger;
        _twitchOAuthClient = new TwitchOAuthClient(clientId, tokensPath)
        {
            Logger = _logger
        };
        _eventSubClient = new EventSubClient(environment)
        {
            Logger = _logger
        };
        _eventSubClient.OnRedeem += InvokeOnRedeem;
        _eventSubClient.Connected += state => OnStateChanged?.Invoke(state ? State.Connected : State.Offline);
    }

    public void Connect() => Task.Run(StartAsync);
    
    public void Disconnect() => _eventSubClient.Disconnect();

    public void Reconnect()
    {
        Disconnect();
        Connect();
    }
    
    private async Task StartAsync()
    {
        _logger?.Invoke("Launching Twitch Client");
        try
        {
            OnStateChanged?.Invoke(State.Pending);
            var session = await _twitchOAuthClient.GetSession();
            
            _eventSubClient.Connect(session);
            OnStateChanged?.Invoke(State.Connected);
        }
        catch (Exception e)
        {
            LogMessage("An error occured while starting Twitch Client: " + e.Message);
            OnStateChanged?.Invoke(State.Offline);
        }
    }
    
    public void Dispose()
    {
        OnStateChanged?.Invoke(State.Offline);
        _eventSubClient.OnRedeem -= InvokeOnRedeem;
        _eventSubClient.Dispose();
    }

    private void InvokeOnRedeem(RedeemEvent e) => OnRedeem?.Invoke(e);
        
    private void LogMessage(string message) => _logger.Invoke($"[TwitchClient] {message}");

    public enum State
    {
        Offline,
        Pending,
        Connected
    }
}