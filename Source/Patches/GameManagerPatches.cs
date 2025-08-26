using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Registers;
using UncertainLuei.CaudexLib.Util.Extensions;

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
