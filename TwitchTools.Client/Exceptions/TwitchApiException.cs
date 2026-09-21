using System;

namespace TwitchTools.Client.Exceptions;

public sealed class TwitchApiException(long statusCode, string message, string errorCode = null) : Exception(message)
{
    public long StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
}