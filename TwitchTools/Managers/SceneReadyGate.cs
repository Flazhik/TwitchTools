using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine.SceneManagement;

namespace TwitchTools.Managers;

[ConfigureSingleton(SingletonFlags.PersistAutoInstance)]
internal sealed class SceneReadyGate : MonoSingleton<SceneReadyGate>
{
    private readonly ConcurrentQueue<Action> _queue = new();
    private bool _sceneLoaded;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        _sceneLoaded = true;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void Run(Action action)
    {
        if (_sceneLoaded)
        {
            action();
            return;
        }

        _queue.Enqueue(action);
    }

    private void OnSceneUnloaded(Scene scene) => _sceneLoaded = false;
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _sceneLoaded = false;
        StartCoroutine(MarkReady());
    }

    /*
     * Just to make sure every MonoSingleton and every other MonoBehavior has had Start() executed
     */
    private IEnumerator MarkReady()
    {
        yield return null;
        _sceneLoaded = true;
        while (_queue.TryDequeue(out var action))
            action();
    }
}