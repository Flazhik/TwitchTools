using System;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace TwitchTools.Client.Utils;

internal static class UnityWebRequestExtension
{
    public static Task SendAsync(this UnityWebRequest request)
    {
        var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var r = request.SendWebRequest();

        r.completed += _ =>
        {
            if (request.result != UnityWebRequest.Result.Success)
                completionSource.SetException(new Exception(request.error));
            else
                completionSource.SetResult(true);
        };

        return completionSource.Task;
    }
}