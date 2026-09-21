using System;
using System.Collections.Concurrent;

namespace TwitchTools.Managers;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
internal class MainThreadDispatcher : MonoSingleton<MainThreadDispatcher>
{
    private static readonly ConcurrentQueue<Action> ExecutionQueue = new();

    public void Enqueue(Action action) => ExecutionQueue.Enqueue(action);
        
    private void Update()
    {
        while (ExecutionQueue.TryDequeue(out var action))
            action();
    }
}