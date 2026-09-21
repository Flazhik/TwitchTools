using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TwitchTools.Client.Environment;
using TwitchTools.Managers;
using UnityEngine.SceneManagement;

namespace TwitchTools;

[BepInProcess("ULTRAKILL.exe")]
[BepInPlugin(PluginInfo.Guid, PluginInfo.Name, PluginInfo.Version)]
public class TwitchToolsPlugin : BaseUnityPlugin
{
    public static readonly TwitchEnvironment Environment = TwitchEnvironment.Production;
    public static ManualLogSource Log;
    private static TwitchToolsManager _manager;

    private Harmony _harmony;

    public void Awake()
    {
        Log = Logger;
        _harmony = new Harmony(PluginInfo.Guid);
        _harmony.PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
        
    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene != SceneManager.GetActiveScene())
            return;
            
        switch (SceneHelper.CurrentScene)
        {
            case "Main Menu":
            {
                _manager = TwitchToolsManager.Instance;
                break;
            }
        }
    }

    private void OnApplicationQuit()
    {
        _manager.TryRestoreInitialGameState();
    }
}