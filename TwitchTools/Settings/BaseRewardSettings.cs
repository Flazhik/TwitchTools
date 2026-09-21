using ThornClient.Core.ConfigurableElements;

namespace TwitchTools.Settings;

public class BaseRewardSettings
{
    public string RewardId { get; }
    public Setting<string> RewardName { get; }
    public Setting<int> Duration { get; }
    public Setting<bool> IsActive { get; }

    public BaseRewardSettings(
        string rewardId,
        Setting<string> rewardName,
        Setting<int> duration,
        Setting<bool> isActive)
    {
        RewardId = rewardId;
        RewardName = rewardName;
        Duration = duration;
        IsActive = isActive;

        IsActive.Value = false;
        IsActive.NotifyChanged();
    }
}