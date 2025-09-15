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
    [HarmonyPatch(typeof(ItemManager), nameof(ItemManager.UseItem))]
    internal static class ItemUsePatch
    {
        private static void ItemUseSuccess(ItemManager im, int selectedItm)
        {
            CaudexEvents.ItemUsed(im, im.items[selectedItm]);
            im.items[selectedItm].OnUseSuccess(im);
        }

        private static bool ExtendedItemUseCheck(Item itm, PlayerManager pm)
        {
            // Item that is being used
            ItemObject itmObj = pm.itm.items[pm.itm.selectedItem];

            // Return true and end if Use function is true
            if (itm.Use(pm)) return true;

            // Immediately return false if it is a CaudexMultiItemObject
            if (itmObj is CaudexMultiItemObject) return false;

            // Perform ItemUsed coroutine when item in slot is changed or has "always_trigger_event" tag
            if (itmObj == pm.itm.items[pm.itm.selectedItem] &&
                (itmObj.GetMeta() == null ||
                !itmObj.GetMeta().tags.Contains("caudex:always_trigger_event")))
                 return false;

            CaudexEvents.ItemUsed(pm.itm, itmObj);
            return false;
        }

        private static readonly MethodInfo itemCheck = AccessTools.Method(typeof(ItemUsePatch), "ExtendedItemUseCheck");
        private static readonly MethodInfo itemUsedMethod = AccessTools.Method(typeof(ItemUsePatch), "ItemUseSuccess");

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;
            bool patched = false;

            for (; i < length; i++)
            {
                if (!patched       &&
                    i + 2 < length &&
                    array[i].opcode   == OpCodes.Callvirt &&
                    array[i+1].opcode == OpCodes.Brfalse  &&
                    array[i+2].opcode == OpCodes.Ldarg_0)
                {
                    patched = true;
                    yield return new CodeInstruction(OpCodes.Call, itemCheck);
                    yield return array[i+1];
                    yield return array[i+2];
                    yield return array[i+3];
                    yield return array[i+4];
                    yield return new CodeInstruction(OpCodes.Call, itemUsedMethod);
                    i += 6;
                    for (; i < length - 1; i++)
                        yield return new CodeInstruction(OpCodes.Nop);
                }
                yield return array[i];
            }
            if (!patched)
                CaudexLibPlugin.Log.LogError("ItemUsePatch.Transpiler transpiler did not get properly applied!");
        }
    }
}
