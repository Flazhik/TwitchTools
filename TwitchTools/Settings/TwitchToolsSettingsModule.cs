using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThornClient.Core;
using ThornClient.Core.ConfigurableElements;
using TwitchTools.Api.Rewards;
using TwitchTools.Api.Settings;
using TwitchTools.Utils;
using UnityEngine;
using Module = ThornClient.Core.Module;

namespace TwitchTools.Settings;

public sealed class TwitchToolsSettingsModule : Module
{
    // TODO: Migrate to asset bundles
    private const string IconBase64 = "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";
    private static readonly MethodInfo RegisterSettingMethod = typeof(TwitchToolsSettingsModule)
        .GetMethod(nameof(RegisterSetting), BindingFlags.Instance | BindingFlags.NonPublic);
    public static TwitchToolsSettingsModule Instance;
        
    public static ConfigButtonRow TwitchConnectionButtons;
    public override Sprite Icon => _icon;

    private readonly Dictionary<Reward, BaseRewardSettings> _rewardsSettings = new();
    private static readonly Sprite _icon;

    static TwitchToolsSettingsModule()
    {
        var imageBytes = Convert.FromBase64String(IconBase64);
        var tex = new Texture2D(92, 92)
        {
            filterMode = FilterMode.Point
        };
        tex.LoadImage(imageBytes);

        _icon = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    public TwitchToolsSettingsModule() : base(PluginInfo.Guid, PluginInfo.Name, "Twitch Rewards settings", ModuleCategory.Utility)
    {
        if (Instance != null)
            return;
            
        Instance = this;
                
        CreateHeader("twitchSettings", "Twitch connection");
        // !!! Do not change this GUID
        CreateHeader("twitchStatusHeader", "Status:   Offline", "", HeaderType.H2);
        TwitchConnectionButtons = CreateButtonRow("twitchConnectionButtons", "Twitch connection buttons", "Issues a new device code",
            ["Sign In", "Disconnect"]);
        CreateHeader("twitchRewards", "Rewards");
    }

    public BaseRewardSettings RegisterRewardSettings(Reward reward)
    {
        if (_rewardsSettings.TryGetValue(reward, out var settings))
            return settings;

        var rewardId = reward.Id;
        var rewardGroup = CreateGroup($"{reward.Id}_group", reward.Name, reward.Description);
        settings = new BaseRewardSettings(
            rewardId,
            CreateSetting($"{rewardId}_rewardName", "Reward name", "Reward name exactly as set on Twitch", "", rewardGroup),
            CreateSetting($"{rewardId}_rewardDuration", "Reward duration", "How long reward stays in effect (sec)", 30, rewardGroup),
            CreateSetting($"{rewardId}_isActive", "Reward is active", "Allows to switch it off manually", false, rewardGroup)
        );

        _rewardsSettings.Add(reward, settings);
        reward.RegisterSettings();

        foreach (var customSetting in reward.Settings)
        {
            RegisterSettingMethod
                .MakeGenericMethod(customSetting.ValueType)
                .Invoke(this, [
                    customSetting,
                    $"{customSetting.RewardId}_{customSetting.Id}",
                    customSetting.Name,
                    customSetting.Description,
                    rewardGroup
                ]);
        }
            
        return settings;
    }

    public BaseRewardSettings GetSettingsByRewardName(string name)
    {
        return _rewardsSettings
            .Where(kv => kv.Value.RewardName.Value.Equals(name))
            .Select(static kv => kv.Value)
            .FirstOrDefault();
    }

    public void SetRewardState(Reward reward, bool state)
    {
        if (!_rewardsSettings.TryGetValue(reward, out var settings))
            return;
                
        settings.IsActive.Value = state;
    }
        
    private void RegisterSetting<T>(
        RewardSetting<T> customSetting,
        string settingId,
        string name,
        string description,
        SettingGroup group)
    {
        var setting = CreateSetting(
            settingId,
            name,
            description,
            customSetting.DefaultValue,
            group);
            
        if (setting.Type == SettingType.Enum)
            setting.Hints = InterfaceHintsFactory.SentenceCaseEnumSubstitutions(setting.Value.GetType());
            
        customSetting.Value = setting.Value;
        setting.OnValueChanged += v => customSetting.Value = v;
    }
}