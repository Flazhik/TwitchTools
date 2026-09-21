using System;
using System.Collections.Generic;
using System.Linq;
using TwitchTools.Api.Rewards;
using TwitchTools.Client.Environment;
using TwitchTools.Client.Models.EventSub;
using TwitchTools.Redeems;
using TwitchTools.Settings;
using TwitchTools.Utils;
using UnityEngine;

namespace TwitchTools.Managers;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
public class RewardsManager : MonoSingleton<RewardsManager>
{
    private readonly Type _baseRewardType = typeof(Reward);
    private readonly Dictionary<string, Reward> _rewards = new();
    private readonly RedeemsStrategy _rewardsStrategy = new OverridableRedeemsStrategy();
    private readonly TwitchEnvironment _environment = TwitchToolsPlugin.Environment;
    
    private SceneReadyGate _sceneReadyGate;
    private TwitchToolsSettingsModule _settings = new();

    public void Awake()
    {
        _settings = TwitchToolsSettingsModule.Instance;
        _sceneReadyGate = SceneReadyGate.Instance;
        _rewardsStrategy.OnRewardStateChanged += OnRewardStateChanged;
            
        DiscoverAndRegisterRewards();
        InvokeRepeating(nameof(SlowUpdate), 0f, 0.1f);
    }
        
    public void Redeem(RedeemEvent redeem)
    {
        TwitchToolsPlugin.Log.LogInfo("[RewardsManager] Redeem event received: " + redeem.Reward.Title);
        if (_settings.GetSettingsByRewardName(redeem.Reward.Title) is not {} rewardSettings
            || !_rewards.TryGetValue(rewardSettings.RewardId, out var reward))
            return;
            
        _sceneReadyGate.Run(() =>
            _rewardsStrategy.OnRedeem(reward, _environment.GetPromptFrom(redeem), Time.time + rewardSettings.Duration.Value));
    }

    public void TryRestoreInitialGameState() => _rewardsStrategy.CancelAll();
        
    private void DisableReward(Reward reward) => _rewardsStrategy.CancelForcefully(reward);
    
    private void SlowUpdate()
    {
        _sceneReadyGate.Run(() => _rewardsStrategy.Update());
    }

    private void OnRewardStateChanged(Reward reward, bool state) => _settings.SetRewardState(reward, state);

    private void DiscoverAndRegisterRewards()
    {
        if (_rewards.Count != 0)
            return;

        var discoveredRewardTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
            where !ReflectionUtils.IsSystemAssembly(assembly)
            from type in ReflectionUtils.SafeGetTypes(assembly)
            where ReflectionUtils.SafeIsAssignableFrom(_baseRewardType, type)
                  && !type.IsInterface
                  && !type.IsAbstract select type).ToList();

        foreach (var rewardType in discoveredRewardTypes)
        {
            if (Activator.CreateInstance(rewardType) is not Reward reward)
                continue;
                
            _rewards.Add(reward.Id, reward);

            var settings = _settings.RegisterRewardSettings(reward);
            settings.IsActive.OnValueChanged += state =>
            {
                if (!state)
                    DisableReward(reward);
            };
                
            TwitchToolsPlugin.Log.LogInfo("[RewardsManager] Registered reward: " + reward.Name);
        }
    }
}