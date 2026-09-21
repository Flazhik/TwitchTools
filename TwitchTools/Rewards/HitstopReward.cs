using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PluginConfig;
using PluginConfig.API;
using PluginConfig.API.Fields;
using TwitchTools.Api.Exceptions;
using TwitchTools.Api.Rewards;
using TwitchTools.Api.Settings;
using TwitchTools.Utils;

namespace TwitchTools.Rewards;

public class HitstopReward : Reward
{
    public override string Id => "dev.flazhik.twitch-tools.rewards.hitstop";
    private const string ConfigGuid = "DaemonWeaponUtils";
    private const string TruestopLengthSetting = "truestopLength";
    private const string HitstopLengthSetting = "hitstopLength";
    private const string SlowdownSetting = "slowdownMult";

    public override string Name => "Hitstop";
    public override string Description => "Changes hitstop multiplier";

    private RewardSetting<float> _minValue;
    private RewardSetting<float> _maxValue;
        
    private FloatField _truestopField;
    private FloatField _hitstopField;
    private FloatField _slowdownField;
        
    private float? _truestopOriginalState;
    private float? _hitstopOriginalState;
    private float? _slowdownOriginalState;

    protected override void Execute(string prompt)
    {
        if (_truestopField == null || _hitstopField == null || _slowdownField == null)
        {
            var config = PluginConfiguratorController.GetConfig(ConfigGuid);
            if (config == null)
                throw new TwitchRewardException("Daemon Weapon Tools config wasn't found, ignoring");
                
            var fields = config.GetPrivate<Dictionary<string, ConfigField>>("fields");

            _truestopField = GetFloatField(TruestopLengthSetting, fields);
            _hitstopField = GetFloatField(HitstopLengthSetting, fields);
            _slowdownField = GetFloatField(SlowdownSetting, fields);

            if (_truestopField == null || _hitstopField == null || _slowdownField == null)
                throw new TwitchRewardException("One of the Daemon Weapon Tools config fields was not found, ignoring");
        }

        var multiplier = TryGetPromptParameter("hitstop", prompt);
        if (!multiplier.HasValue)
            throw new TwitchRewardException("Hitstop parameter is missing from the user's prompt, ignoring");
            
        if (multiplier < _minValue?.Value || multiplier > _maxValue?.Value)
            throw new TwitchRewardException("Hitstop parameter value is outside the selected bounds");

        _truestopOriginalState = _truestopField.value;
        _truestopField.value = multiplier.Value;
        _truestopField.TriggerValueChangeEvent();
            
        _hitstopOriginalState = _hitstopField.value;
        _hitstopField.value = multiplier.Value;
        _hitstopField.TriggerValueChangeEvent();
            
        _slowdownOriginalState = _slowdownField.value;
        _slowdownField.value = multiplier.Value;
        _slowdownField.TriggerValueChangeEvent();
        return;

        FloatField GetFloatField(string key, Dictionary<string, ConfigField> fields)
        {
            return fields
                .Where(field => field.Key.Equals(key))
                .Select(static field => field.Value)
                .Where(static field => field.GetType().IsAssignableFrom(typeof(FloatField)))
                .Select(static field => (FloatField)field)
                .FirstOrDefault();
        }
    }
        
    protected override void CancelOut()
    {
        if (_truestopOriginalState.HasValue)
        {
            _truestopField.value = _truestopOriginalState.Value;
            _truestopOriginalState = null;
        }

        _truestopField.TriggerValueChangeEvent();
            
        if (_hitstopOriginalState.HasValue)
        {
            _hitstopField.value = _hitstopOriginalState.Value;
            _hitstopOriginalState = null;
        }

        _hitstopField.TriggerValueChangeEvent();
            
        if (_slowdownOriginalState.HasValue)
        {
            _slowdownField.value = _slowdownOriginalState.Value;
            _slowdownOriginalState = null;
        }

        _slowdownField.TriggerValueChangeEvent();
    }

    private static float? TryGetPromptParameter(string parameter, string prompt)
    {
        var matches = Regex.Matches(prompt, @$"{parameter}\s*\:\s*([0-9\.]+)", RegexOptions.IgnoreCase);
        if (matches.Count == 0)
            return null;

        var value = matches[0].Groups[1].Value;
        return float.TryParse(value, out var i) ? i : null;
    }
        
    public override void RegisterSettings()
    {
        _minValue = CreateSetting("hitstopMinValue", "Hitstop multiplier min value", "Minimum value of hitstop multiplier",
            0f);
            
        _maxValue = CreateSetting("hitstopMaxValue", "Hitstop multiplier max value", "Maximum value of hitstop multiplier",
            3f);
    }
}