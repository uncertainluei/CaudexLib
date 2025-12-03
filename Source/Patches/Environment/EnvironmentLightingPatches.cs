using HarmonyLib;

using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch]
    static class EnvironmentLightingPatches
    {
        [HarmonyPatch(typeof(EnvironmentController), "Awake"), HarmonyPostfix]
        private static void AddLightmapModHolder(EnvironmentController __instance)
        {
            LightmapModHolder holder = __instance.gameObject.AddComponent<LightmapModHolder>();
            holder.Environment = __instance;
            LightmapModHolder._instances.Add(__instance, holder);
        }

        [HarmonyPatch(typeof(LightController), "UpdateLighting"), HarmonyPostfix]
        private static void LightingUpdateTranspiler(LightController __instance, ref Color ___color, ref float ___level)
        {
            LightmapModHolder holder = LightmapModHolder.GetInstance(__instance.ec);            
            if (holder == null) return;
            holder.Invoke(ref ___color);
            ___color.r = Mathf.Clamp(___color.r, 0, 1);
            ___color.g = Mathf.Clamp(___color.g, 0, 1);
            ___color.b = Mathf.Clamp(___color.b, 0, 1);
            ___level = Mathf.Max(___color.r, ___color.g, ___color.b);
        }
    }
}
