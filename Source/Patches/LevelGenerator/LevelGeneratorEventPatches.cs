using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

using UncertainLuei.CaudexLib.Registers;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(LevelGenerator))]
    static class LevelGeneratorEventPatches
    {
        private static readonly MethodInfo invokeEventMethod = AccessTools.Method(typeof(CaudexGeneratorEvents), "Invoke");

        [HarmonyPatch("StartGenerate"), HarmonyPostfix]
        private static void StartGenerate(LevelGenerator __instance)
            => CaudexGeneratorEvents.Invoke(__instance, CaudexGeneratorEventType.Start);

        [HarmonyPatch("Generate", MethodType.Enumerator), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> EventTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            byte patchesLeft = 2;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length && patchesLeft == 2; i++)
            {
                // if (levelGenerator.ld.potentialBaldis.Length != 0)
                if (array[i].opcode   == OpCodes.Ldloc_2    &&
                    array[i+1].opcode == OpCodes.Ldfld      &&
                    array[i+2].opcode == OpCodes.Ldfld      &&
                    array[i+3].opcode == OpCodes.Ldlen      &&
                    array[i+4].opcode == OpCodes.Brfalse)
                {
                    patchesLeft--;
                    // CaudexGeneratorEvents.Invoke(this, CaudexGeneratorEventType.NpcPrep);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    yield return new CodeInstruction(OpCodes.Call, invokeEventMethod);
                }
                yield return array[i];
            }
            for (; i < length && patchesLeft == 1; i++)
            {
                // levelInProgress = false;
                // levelCreated = true;
                // if (CoreGameManager.Instance.GetCamera(0) != null)
                //     CoreGameManager.Instance.GetCamera(0).StopRendering(false);
                if (array[i].opcode    == OpCodes.Ldloc_2   &&
                    array[i+1].opcode  == OpCodes.Ldc_I4_0  &&
                    array[i+2].opcode  == OpCodes.Stfld     &&
                    array[i+3].opcode  == OpCodes.Ldloc_2   &&
                    array[i+4].opcode  == OpCodes.Ldc_I4_1  &&
                    array[i+5].opcode  == OpCodes.Stfld     &&
                    array[i+6].opcode  == OpCodes.Call      &&
                    array[i+7].opcode  == OpCodes.Ldc_I4_0  &&
                    array[i+8].opcode  == OpCodes.Callvirt  &&
                    array[i+9].opcode  == OpCodes.Ldnull    &&
                    array[i+10].opcode == OpCodes.Call      &&
                    array[i+11].opcode == OpCodes.Brfalse   &&
                    array[i+12].opcode == OpCodes.Call      &&
                    array[i+13].opcode == OpCodes.Ldc_I4_0  &&
                    array[i+14].opcode == OpCodes.Callvirt  &&
                    array[i+13].opcode == OpCodes.Ldc_I4_0  &&
                    array[i+14].opcode == OpCodes.Callvirt)
                {
                    patchesLeft--;
                    // CaudexGeneratorEvents.Invoke(this, CaudexGeneratorEventType.Finalizer);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_2);
                    yield return new CodeInstruction(OpCodes.Call, invokeEventMethod);
                }
                yield return array[i];
            }
            for (; i < length; i++)
            {
                yield return array[i];
            }

            if (patchesLeft > 0)
                CaudexLibPlugin.Log.LogError("Transpiler \"CaudexLib.LevelGeneratorEventPatches.EventTranspiler\" wasn't properly applied!");

            yield break;
        }
    }
}
