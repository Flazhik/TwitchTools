using System;

namespace TwitchTools.Api.Exceptions;

public class TwitchRewardException : Exception
{
    public TwitchRewardException(string message) : base(message)
    {
    }
        
    public TwitchRewardException(string message, Exception e) : base(message, e)
    {
    }
}