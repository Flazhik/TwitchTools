using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TwitchTools.Client.Exceptions;
using TwitchTools.Client.Models.OAuth;
using UnityEngine;
using UnityEngine.Networking;

namespace TwitchTools.Client.Clients.OAuth;

internal sealed class TwitchOAuthClient(string clientId, string tokenPath)
{
    private const string OAuthUrl = "https://id.twitch.tv/oauth2";

    private static readonly string[] Scopes =
    [
        "channel:read:redemptions"
    ];

    public Action<string> Logger { get; set; }
        
    public async Task<TwitchSession> GetSession()
    {
        var token = LoadToken();
        if (token == null || token.IsExpired)
        {
            LogMessage("Token not found or expired");
            await RegisterDeviceAndSaveToken();
        } else {
            try
            {
                await Validate(token.AccessToken);
            }
            catch (TwitchApiException)
            {
                LogMessage("Token expired");
                await RegisterDeviceAndSaveToken();
            }
        }

        var validation = await Validate(token.AccessToken);

        return new TwitchSession(
            token.AccessToken,
            validation.UserId,
            validation.Login,
            validation.ClientId);

        async Task RegisterDeviceAndSaveToken()
        {
            token = await RegisterDevice();
            SaveToken(token);
        }
    }

    private async Task<TwitchToken> RegisterDevice()
    {
        var form = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "scopes", string.Join(" ", Scopes) }
        };

        using var request = UnityWebRequest.Post(OAuthUrl + "/device", form);
        var deviceCode = await SendAsync<DeviceCodeResponse>(request);
        Application.OpenURL(deviceCode.VerificationUri);
            
        return await StartTokenPolling(deviceCode);
    }
        
    private async Task<TwitchToken> StartTokenPolling(DeviceCodeResponse deviceCode)
    {
        LogMessage($"Starting device authorization polling [interval={deviceCode.Interval}s]");
        var interval = deviceCode.Interval;
            
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(interval));
            var form = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "device_code", deviceCode.DeviceCode },
                { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" }
            };
                
            using var request = UnityWebRequest.Post(OAuthUrl + "/token", form);
            try
            {
                var response = await SendAsync<TokenResponse>(request);
                return TwitchToken.From(response);
            }
            catch (TwitchApiException ex)
            {
                switch (ex.Message)
                {
                    case "authorization_pending":
                        continue;

                    case "slow_down":
                        interval += 5;
                        continue;

                    default:
                        throw;
                }
            }
        }
    }

    // This shit refuses to work smh
        
    /*private async Task<TwitchToken> TokenRefresh(TwitchToken token)
    {
        var form = new Dictionary<string, string>
        {
            { "client_id", _clientId },
            { "grant_type", "refresh_token" },
            { "refresh_token", token.RefreshToken }
        };

        using var request = UnityWebRequest.Post(OAuthUrl + "/token", form);
        var response = await SendAsync<TokenResponse>(request);
        return TwitchToken.From(response);
    }*/

    private async Task<ValidateResponse> Validate(string accessToken)
    {
        using var request = UnityWebRequest.Get(OAuthUrl + "/validate");
        request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
        return await SendAsync<ValidateResponse>(request);
    }
        
    private async Task<T> SendAsync<T>(UnityWebRequest request)
    {
        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
            return JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            
        OAuthErrorResponse error = null;

        if (!string.IsNullOrEmpty(request.downloadHandler.text))
        {
            try
            {
                error = JsonConvert.DeserializeObject<OAuthErrorResponse>(request.downloadHandler.text);
            }
            catch
            {
                LogMessage($"[{request.responseCode}] Couldn't parse error response: {request.downloadHandler.text}");
            }
        }
                
        var e = new TwitchApiException(
            request.responseCode,
            error?.Message ??
            request.error ??
            "Unknown Twitch error",
            error?.Error);

        LogMessage($"[{e.StatusCode}] An error occured [code={e.ErrorCode ?? "N/A"}]: {e.Message}");
        throw e;
    }

    private TwitchToken LoadToken()
    {
        return !File.Exists(tokenPath)
            ? null
            : JsonConvert.DeserializeObject<TwitchToken>(File.ReadAllText(tokenPath));
    }

    private void SaveToken(TwitchToken token)
    {
        File.WriteAllText(tokenPath, JsonConvert.SerializeObject(token, Formatting.Indented));
        LogMessage("Token saved");
    }

    private void LogMessage(string message) => Logger?.Invoke("[OAuth] " + message);
}