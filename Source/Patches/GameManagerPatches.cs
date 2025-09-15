using HarmonyLib;
using UncertainLuei.CaudexLib.Registers;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(BaseGameManager))]
    internal static class GameManagerPatches
    {
        [HarmonyPatch("CollectNotebooks"), HarmonyPostfix]
        private static void InvokeCollectNotebooks()
            => CaudexEvents.NotebookCollected();
    }
}
