namespace TwitchTools.Client;

public sealed class TwitchSession(
    string accessToken,
    string userId,
    string login,
    string clientId)
{
    public string AccessToken { get; } = accessToken;
    public string UserId { get; } = userId;
    public string Login { get; } = login;
    public string ClientId { get; } = clientId;
}