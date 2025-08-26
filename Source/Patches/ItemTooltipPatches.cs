using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch]
    public static class ItemTooltipPatches
    {
        private static readonly FieldInfo nameKeyField = AccessTools.Field(typeof(ItemObject), "nameKey");
        private static readonly MethodInfo getNameMethod = AccessTools.Method(typeof(ItemObjectExtensions), "GetName");

        private static readonly FieldInfo descKeyField = AccessTools.Field(typeof(ItemObject), "descKey");
        private static readonly MethodInfo getDescMethod = AccessTools.Method(typeof(ItemObjectExtensions), "GetDescription");

        public static IEnumerable<CodeInstruction> ReplaceReferencesName(this IEnumerable<CodeInstruction> instructions, bool localize = true)
            => instructions.ReplaceReferences(nameKeyField, getNameMethod, localize);

        public static IEnumerable<CodeInstruction> ReplaceReferencesDesc(this IEnumerable<CodeInstruction> instructions, bool localize = true)
            => instructions.ReplaceReferences(descKeyField, getDescMethod, localize);

        internal static IEnumerable<CodeInstruction> ReplaceReferences(this IEnumerable<CodeInstruction> instructions, FieldInfo target, MethodInfo replacement, bool localize = true)
        {
            object targetAsObj = target;
            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0, patches = 0;

            for (; i < length; i++)
            {
                if (array[i].opcode != OpCodes.Ldfld ||
                    array[i].operand != targetAsObj)
                {
                    yield return array[i];
                    continue;
                }

                patches++;
                yield return new CodeInstruction(localize ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                yield return new CodeInstruction(OpCodes.Call, replacement);
            }
            if (patches == 0)
                CaudexLibPlugin.Log.LogError("ItemTooltipPatches.ReplaceReferences transpiler did not find anything to patch!");
        }

        private static readonly object localeManagerInstance = AccessTools.PropertyGetter(typeof(LocalizationManager), "Instance");
        private static readonly object getLocaleTextInstance = AccessTools.Method(typeof(LocalizationManager), "GetLocalizedText", [typeof(string)]);

        internal static IEnumerable<CodeInstruction> ReplaceDirectLocalization(this IEnumerable<CodeInstruction> instructions, FieldInfo target, MethodInfo replacement)
        {
            object targetAsObj = target;

            CodeInstruction[] array = instructions.ToArray();
            List<CodeInstruction> insideInstructs = [];
            int length = array.Length, i = 0, patches = 0;

            for (; i < length; i++)
            {
                if (array[i].opcode == OpCodes.Call &&
                    array[i].operand == localeManagerInstance)
                {
                    // Try finding GetLocalizedText(MYFIELD)
                    bool found = false;
                    insideInstructs.Clear();
                    for (int j = i + 1; j < length && !found; j++)
                    {
                        if (array[j].opcode == OpCodes.Ldfld &&
                            array[j].operand == targetAsObj &&
                            array[j + 1].opcode == OpCodes.Callvirt &&
                            array[j + 1].operand == getLocaleTextInstance)
                        {
                            found = true;
                            patches++;
                            foreach (CodeInstruction ins in insideInstructs)
                                yield return ins;
                            yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                            yield return new CodeInstruction(OpCodes.Call, replacement);
                            i = j + 2;
                            continue;
                        }
                        insideInstructs.Add(array[j]);
                    }
                }
                yield return array[i];
            }
            if (patches == 0)
                CaudexLibPlugin.Log.LogError("ItemTooltipPatches.ReplaceDirectLocalization transpiler did not find anything to patch!");
        }

        [HarmonyPatch(typeof(HudManager), "SetItemSelect"), HarmonyTranspiler]
        //[HarmonyPatch(typeof(TooltipController), "ActualUpdateTooltip")]
        internal static IEnumerable<CodeInstruction> RemoveLocalizers(this IEnumerable<CodeInstruction> instructions)
        {
            CodeInstruction[] array = instructions.ToArray();
            List<CodeInstruction> insideInstructs = [];
            int length = array.Length, i = 0, patches = 0;

            for (; i < length; i++)
            {
                if (array[i].opcode == OpCodes.Call &&
                    array[i].operand == localeManagerInstance)
                {
                    // Try finding GetLocalizedText(MYFIELD)
                    bool found = false;
                    insideInstructs.Clear();
                    for (int j = i+1; j < length && !found; j++)
                    {
                        if (array[j].opcode == OpCodes.Callvirt &&
                            array[j].operand == getLocaleTextInstance)
                        {
                            found = true;
                            patches++;
                            foreach (CodeInstruction ins in insideInstructs)
                                yield return ins;
                            i = j+1;
                            continue;
                        }
                        insideInstructs.Add(array[j]);
                    }
                }
                yield return array[i];
            }
            if (patches == 0)
                CaudexLibPlugin.Log.LogError("ItemTooltipPatches.ReplaceDirectLocalization transpiler did not find anything to patch!");
        }

        [HarmonyPatch(typeof(ItemManager), "UpdateSelect"), HarmonyTranspiler]
        [HarmonyPatch(typeof(ItemMetaData), "nameKey", MethodType.Getter)]
        private static IEnumerable<CodeInstruction> BaseLocalizedNames(IEnumerable<CodeInstruction> instructions)
            => instructions.ReplaceReferencesName(true);

        [HarmonyPatch(typeof(LevelGenerator), "Generate", MethodType.Enumerator), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BaseUnlocalizedNames(IEnumerable<CodeInstruction> instructions)
            => instructions.ReplaceReferencesName(false);

        [HarmonyPatch(typeof(Pickup), "ClickableSighted"), HarmonyTranspiler]
        [HarmonyPatch(typeof(StoreScreen), "InventoryDescription")]
        private static IEnumerable<CodeInstruction> BaseLocalizedDescs(IEnumerable<CodeInstruction> instructions)
            => instructions.ReplaceReferencesDesc(true);

        [HarmonyPatch(typeof(StoreScreen), "UpdateDescription"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BaseReplacementDescs(IEnumerable<CodeInstruction> instructions)
            => instructions.ReplaceDirectLocalization(descKeyField, getDescMethod);
    }
}