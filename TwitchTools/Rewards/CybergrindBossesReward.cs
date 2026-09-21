using System.Collections.Generic;
using TwitchTools.Api.Exceptions;
using TwitchTools.Api.Rewards;
using TwitchTools.Utils;

namespace TwitchTools.Rewards;

public class CybergrindBossesReward : Reward
{
    private const string CheatId = "nyxpiri.cybergrind-bosses";
    private const string KeepCheatsEnabledId = "ultrakill.keep-enabled";
        
    public override string Id => "dev.flazhik.twitch-tools.rewards.cybergrind-bosses";
    public override string Name => "Cybergrind bosses";
    public override string Description => "Modifies PSX settings";

    private CheatsController _cheatsController;
    private CheatsManager _cheatsManager;
    private AssistController _assists;
    private bool? _originalState;
    private bool? _originalKeepCheatsEnabledState;

    protected override void Execute(string prompt)
    {
        if (SceneHelper.CurrentScene != "Endless")
            throw new TwitchRewardException("Not currently in Cybergrind, ignoring");
            
        _cheatsController = CheatsController.Instance!;
        _cheatsManager = CheatsManager.Instance!;
        _assists = AssistController.Instance!;
            
        if (!_assists.cheatsEnabled)
            _cheatsController.ActivateCheats();
            
        if (!TryGetCheat(CheatId, out var cheat))
            throw new TwitchRewardException("Cybergrind bosses cheat not found, ignoring");

        _originalState = _cheatsManager.GetCheatState(CheatId);
        _originalKeepCheatsEnabledState = _cheatsManager.GetCheatState(KeepCheatsEnabledId);
            
        if (!_originalState.Value)
            _cheatsManager.ToggleCheat(cheat);
            
        if (!_originalKeepCheatsEnabledState.Value && TryGetCheat(KeepCheatsEnabledId, out var keepEnabled))
            _cheatsManager.ToggleCheat(keepEnabled);
    }
        
    protected override void CancelOut()
    {
        _cheatsManager = CheatsManager.Instance!;
            
        if (_originalState.HasValue && !_originalState.Value
                                    && TryGetCheat(CheatId, out var cheat)
                                    && _cheatsManager.GetCheatState(CheatId))
        {
            _cheatsManager.ToggleCheat(cheat);
            _originalState = null;
        }
            
            
        if (_originalKeepCheatsEnabledState.HasValue
            && !_originalKeepCheatsEnabledState.Value
            && TryGetCheat(KeepCheatsEnabledId, out var keepEnabled)
            && _cheatsManager.GetCheatState(KeepCheatsEnabledId))
        {
            _cheatsManager.ToggleCheat(keepEnabled);
            _originalKeepCheatsEnabledState = null;
        }
    }

    private bool TryGetCheat(string key, out ICheat result)
    {
        var cheats = _cheatsManager.GetPrivate<Dictionary<string, ICheat>>("idToCheat");
        return cheats.TryGetValue(key, out result);
    }
        
    public override void RegisterSettings()
    {
        // No extra settings
    }
}