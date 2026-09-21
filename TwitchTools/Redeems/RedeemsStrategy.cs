using System;
using TwitchTools.Api.Rewards;

namespace TwitchTools.Redeems;

public abstract class RedeemsStrategy : IDisposable
{
    public Action<Reward, bool> OnRewardStateChanged;
        
    public abstract void OnRedeem(Reward reward, string prompt, float cutoff);

    public abstract void CancelForcefully(Reward reward);
        
    public abstract void Dispose();
        
    public abstract void Update();

    public abstract void CancelAll();
}