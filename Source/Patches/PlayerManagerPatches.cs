using HarmonyLib;
using UncertainLuei.CaudexLib.Components;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(PlayerManager))]
    internal static class PlayerManagerPatches
    {
        [HarmonyPatch("Start"), HarmonyPostfix]
        private static void Start(PlayerManager __instance)
        {
            __instance.gameObject.AddComponent<PlayerSilenceManager>();
        }
    }
}
