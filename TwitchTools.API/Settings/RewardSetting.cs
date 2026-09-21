using System;

namespace TwitchTools.Api.Settings;

public abstract class RewardSetting(string id, string rewardId, string name, string description)
{
    public string Id { get; private set; } = id;
    public string RewardId { get; private set; } = rewardId;
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;

    public abstract Type ValueType { get; }
}
    
public class RewardSetting<T>(string rewardId, string id, string name, string description, T defaultValue)
    : RewardSetting(rewardId, id, name, description)
{
    public T Value;
    public override Type ValueType => typeof(T);
    public T DefaultValue { get; } = defaultValue;
}