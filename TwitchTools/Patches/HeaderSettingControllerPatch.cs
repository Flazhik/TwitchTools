using System.Reflection;
using HarmonyLib;
using TwitchTools.Managers;

namespace TwitchTools.Patches;

/*
 * Forgive me, Father, for I have sinned
 *
 * I'll be anxiously waiting for custom UI elements in Thorn
 */
[HarmonyPatch]
internal class HeaderSettingControllerPatch
{
    private static MethodBase TargetMethod()
    {
        var type = AccessTools.TypeByName("ThornClient.System.ClickGUIComponents.HeaderSettingController");
        return AccessTools.Method(type, "Start");
    }

    private static void Postfix()
    {
        if (ConfigHeaderUICreatorPatch.TwitchStatusText != null)
            ConfigHeaderUICreatorPatch.TwitchStatusText.text = TwitchToolsManager.Instance?.twitchClientState;
    }
}