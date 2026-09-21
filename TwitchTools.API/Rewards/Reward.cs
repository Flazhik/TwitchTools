using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TwitchTools.Api.Exceptions;
using TwitchTools.Api.Settings;

namespace TwitchTools.Api.Rewards;

public abstract class Reward
{
    public IReadOnlyList<RewardSetting> Settings => _settings;
    private readonly List<RewardSetting> _settings = [];
    
    private readonly object _lock = new();
        
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
        
    public abstract void RegisterSettings();
    protected abstract void Execute(string prompt);
    protected abstract void CancelOut();
        
    public void Enable(string prompt)
    {
        lock (_lock)
        {
            try
            {
                Execute(prompt);
            }
            catch (TwitchRewardException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new TwitchRewardException($"Unexpected error while trying to execute reward {Name}", e);
            }
        }
    }
        
    public void Cancel()
    {
        lock (_lock)
        {
            CancelOut();
        }
    }

    protected RewardSetting<T> CreateSetting<T>(string id, string name, string description, T defaultValue)
    {
        var setting = new RewardSetting<T>(id, Id, name, description, defaultValue);
        _settings.Add(setting);
        return setting;
    }
        
    public override bool Equals(object obj) => obj is Reward other && Id == other.Id;
        
    public override int GetHashCode() => Id.GetHashCode();
}