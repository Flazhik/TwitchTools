using System;
using System.IO;
using ThornClient.Managers;
using TwitchTools.Client.Clients;
using TwitchTools.Patches;
using TwitchTools.Settings;
using TwitchTools.Utils;

namespace TwitchTools.Managers;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
internal class TwitchToolsManager: MonoSingleton<TwitchToolsManager>
{
    // TODO: Make clientId a tweakable parameter
    private const string ClientId = "dyn8vl8ccgzjef03wcbj1oz130zql0";
    public string twitchClientState = "Status:   Offline";
    
    private static string TokensPath => Path.Combine(PathUtils.AssemblyPath, "twitch_tokens");
    private Action<string> Logger => msg => _mainThread.Enqueue(() => TwitchToolsPlugin.Log.LogInfo(msg));
        
    private TwitchClient _client;
    private TwitchToolsSettingsModule _settingsModule;
    private MainThreadDispatcher _mainThread;
    private RewardsManager _rewardsManager;

    private void Awake()
    {
        _settingsModule = TwitchToolsSettingsModule.Instance!;
        _mainThread = MainThreadDispatcher.Instance!;
        _rewardsManager = RewardsManager.Instance!;
        
        _client = new TwitchClient(ClientId, TokensPath, Logger, TwitchToolsPlugin.Environment);
        _client.OnRedeem += e => _mainThread.Enqueue(() => _rewardsManager.Redeem(e));
        _client.OnStateChanged += OnClientStateChanged;
        
        TwitchToolsSettingsModule.TwitchConnectionButtons.OnClick += index =>
        {
            switch (index)
            {
                case 0:
                    _client.Reconnect();
                    break;
                case 1:
                    _client.Disconnect();
                    break;
            }
        };
            
        ConfigManager.LoadConfig(_settingsModule);
    }

    public void TryRestoreInitialGameState() => _rewardsManager.TryRestoreInitialGameState();

    private void OnClientStateChanged(TwitchClient.State state)
    {
        if (ConfigHeaderUICreatorPatch.TwitchStatusText == null)
            return;
                
        var statusTxt = state switch
        {
            TwitchClient.State.Offline => "Offline",
            TwitchClient.State.Pending => "<color=#e5eb44>Pending</color>",
            TwitchClient.State.Connected => "<color=#44ff45>Connected</color>",
            _ => "Unknown"
        };

        twitchClientState = $"Status:   {statusTxt}";
        ConfigHeaderUICreatorPatch.TwitchStatusText.text = twitchClientState;
    }
}