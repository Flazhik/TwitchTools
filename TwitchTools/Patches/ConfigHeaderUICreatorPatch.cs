using HarmonyLib;
using ThornClient.Core.ConfigurableElements;
using TMPro;
using TwitchTools.Managers;
using UnityEngine;

namespace TwitchTools.Patches;

/*
 * Forgive me, Father, for I have sinned
 *
 * I'll be anxiously waiting for custom UI elements in Thorn
 */
[HarmonyPatch(typeof(ConfigHeaderUICreator))]
internal class ConfigHeaderUICreatorPatch
{
    public static TextMeshProUGUI TwitchStatusText;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ConfigHeaderUICreator), nameof(ConfigHeaderUICreator.CreateUI))]
    public static void ConfigHeaderUICreator_CreateUI_Postfix(ConfigHeaderUICreator __instance, ConfigHeader element, ref GameObject __result)
    {
        if (!element.GUID.Equals("twitchStatusHeader"))
            return;
            
        TwitchStatusText = __result.transform.Find("Name").GetComponent<TextMeshProUGUI>();
        TwitchStatusText.text = TwitchToolsManager.Instance!.twitchClientState;
    }
}