using BepInEx;
using HarmonyLib;

using MTM101BaldAPI.AssetTools;
using UncertainLuei.CaudexLib.Util;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(AssetLoader))]
    internal static class ApiAssetLoaderPatches
    {
        [HarmonyPatch("GetModPath"), HarmonyPostfix]
        private static void GetExtendedModPath(BaseUnityPlugin plug, ref string __result)
        {
            if (CaudexAssetLoader.modPathOverrides.TryGetValue(plug.Info, out string val))
                __result = val;
        }
    }
}
