using System.Collections.Concurrent;
using System.Linq;
using TwitchTools.Api.Exceptions;
using TwitchTools.Api.Rewards;
using UnityEngine;

namespace TwitchTools.Redeems;

internal class OverridableRedeemsStrategy : RedeemsStrategy
{
    private readonly ConcurrentDictionary<Reward, float> _activeRewards = new();

    public override void OnRedeem(Reward reward, string prompt, float cutoff)
    {
        TryRemoveReward(reward);
        if (!_activeRewards.TryAdd(reward, cutoff))
            return;
            
        try
        {
            reward.Enable(prompt);
            OnRewardStateChanged?.Invoke(reward, true);
        }
        catch (TwitchRewardException e)
        {
            TwitchToolsPlugin.Log.LogError(e);
            TryRemoveReward(reward);
        }
    }

    public override void CancelForcefully(Reward reward) =>
        TryRemoveReward(reward, true);

    public override void Update()
    {
        foreach (var activeReward in _activeRewards.Where(static activeReward => !(activeReward.Value > Time.time)))
            TryRemoveReward(activeReward.Key);
    }
    
    public override void CancelAll()
    {
        foreach (var activeReward in _activeRewards)
            CancelForcefully(activeReward.Key);
    }

    public override void Dispose()
    {
        var activeRewards = _activeRewards.Keys;
        _activeRewards.Clear();

        foreach (var reward in activeRewards)
        {
            reward.Cancel();
            OnRewardStateChanged?.Invoke(reward, false);
        }
    }
        
    private void TryRemoveReward(Reward reward, bool silent = false)
    {
        if (_activeRewards.TryRemove(reward, out _))
            reward.Cancel();
            
        if (!silent)
            OnRewardStateChanged?.Invoke(reward, false);
    }
}