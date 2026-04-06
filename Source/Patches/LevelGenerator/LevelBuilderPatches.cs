using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

using UncertainLuei.CaudexLib.Registers;
using MTM101BaldAPI;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(LevelBuilder))]
    static class LevelBuilderPatches
    {
        [HarmonyPatch("AddNpcsFromPreviousLevels"), HarmonyPostfix]
        private static void AddSceneObjectNpcs(LevelBuilder __instance, SceneObject ___scene, LevelGenerationParameters ___ld, EnvironmentController ___ec)
        {
            ___ec.npcsToSpawn.AddRange(___scene.forcedNpcs);

            List<WeightedSelection<NPC>> npcs = new(___scene.potentialNPCs);
            if (___ld is CustomLevelGenerationParameters customLd)
                npcs.AddRange(customLd.GetPotentialNpcs());
            
            foreach (NPC npc in ___ec.npcsToSpawn)
            {
                for (int i = 0; i < npcs.Count; i++)
                {
                    if (npcs[i].selection != npc) continue;

                    npcs.RemoveAt(i);
                    i--;
                }
            }

            for (int j = 0, idx; npcs.Count > 0 && j < ___scene.additionalNPCs; j++)
            {
                idx = WeightedSelection<NPC>.ControlledRandomIndexList(npcs, __instance.controlledRNG);
                ___ec.npcsToSpawn.Add(npcs[idx].selection);
                npcs.RemoveAt(idx);
            }
        }

        [HarmonyPatch("AddNpcsFromPreviousLevels"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> NpcAddTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length && !patched; i++)
            {
                yield return array[i];
                
                // list.Add(scene);
                if (array[i].opcode   == OpCodes.Dup        &&
                    array[i+1].opcode == OpCodes.Ldarg_0    &&
                    array[i+2].opcode == OpCodes.Ldfld      &&
                    array[i+3].opcode == OpCodes.Callvirt)
                {
                    // Skip function
                    patched = true;
                    i += 3;
                }
            }
            for (; i < length; i++)
                yield return array[i];

            if (!patched)
                CaudexLibPlugin.Log.LogError("Transpiler \"CaudexLib.LevelBuilderPatches.NpcAddTranspiler\" wasn't properly applied!");

            yield break;
        }
    }
}
