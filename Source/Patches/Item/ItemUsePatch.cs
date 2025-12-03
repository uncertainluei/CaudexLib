using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Registers;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.UseItem))]
    internal static class ItemUsePatch
    {

        private static bool ExtendedItemUseCheck(Item itm, PlayerManager pm)
        {
            // Item that is being used
            ItemObject itmObj = pm.itm.items[pm.itm.selectedItem];

            // Return true and end if Use function is true
            if (!itm.Use(pm))
            {
                // Perform ItemUsed coroutine when item in slot is changed or has "always_trigger_event" tag
                if (itmObj == pm.itm.items[pm.itm.selectedItem] &&
                    itmObj.GetMeta()?.tags.Contains("caudex:always_trigger_event") != true)
                    return false;

                CaudexEvents.ItemUsed(pm.itm, itmObj);
                return false;
            }

            // Run ItemUsed event now that we know it is deemed as successful in vanilla
            CaudexEvents.ItemUsed(pm.itm, itmObj);

            if (!itmObj.OverrideUseResult(pm.itm)) return true;

            itm?.PostUse(pm);
            return false;
        }

        private static readonly MethodInfo itemCheck = AccessTools.Method(typeof(ItemUsePatch), "ExtendedItemUseCheck");
        private static readonly object useItem = AccessTools.Method(typeof(Item), "Use");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;
            bool patched = false;

            for (; i < length; i++)
            {
                if (!patched && i+1 < length &&
                    array[i].opcode   == OpCodes.Callvirt &&
                    array[i].operand  == useItem          &&
                    array[i+1].opcode == OpCodes.Brfalse)
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Call, itemCheck);
                    continue;
                }
                yield return array[i];
            }
            if (!patched)
                CaudexLibPlugin.Log.LogError("ItemUsePatch.Transpiler transpiler did not get properly applied!");
        }
    }
}
