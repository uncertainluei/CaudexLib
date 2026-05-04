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
        private static readonly object potentialNPCs = AccessTools.Field(typeof(SceneObject), "potentialNPCs");
        private static readonly MethodInfo addForcedNpcsFromLvl = AccessTools.Method(typeof(LevelBuilderPatches), "AddForcedNpcsFromLvl");
        private static readonly MethodInfo addPotentialNpcsFromLvl = AccessTools.Method(typeof(LevelBuilderPatches), "AddPotentialNpcsFromLvl");


        [HarmonyPatch("AddNpcsFromPreviousLevels"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> NpcAddTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length && !patched; i++)
            {
                // List<WeightedNPC> potentialNpcs = new List<WeightedNPC>(sceneObject.potentialNPCs);
                if (i+4 < length &&
                    array[i].opcode   == OpCodes.Ldloc_1    &&
                    array[i+1].opcode == OpCodes.Ldfld      &&
                    array[i+1].operand == potentialNPCs     &&
                    array[i+2].opcode == OpCodes.Newobj     &&
                    array[i+3].opcode == OpCodes.Stloc_2)
                {
                    // AddForcedNpcsFromLvl(this, sceneObject);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Call, addForcedNpcsFromLvl);
                    // List<WeightedNPC> potentialNpcs = new List<WeightedNPC>(sceneObject.potentialNPCs);
                    yield return array[i];
                    yield return array[i+1];
                    yield return array[i+2];
                    yield return array[i+3];
                    i += 4;
                    // AddPotentialNpcsFromLvl(this, sceneObject, potentialNpcs);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, addPotentialNpcsFromLvl);
                    patched = true;
                    break;
                }
                yield return array[i];
            }
            for (; i < length; i++)
                yield return array[i];

            if (!patched)
                CaudexLibPlugin.Log.LogError("Transpiler \"CaudexLib.LevelBuilderPatches.NpcAddTranspiler\" wasn't properly applied!");

            yield break;
        }

        private static CustomLevelObject _levelObject;
        private static void AddForcedNpcsFromLvl(LevelBuilder builder, SceneObject scene)
        {
            _levelObject = null;

            if (scene == builder.scene) // "scene" refers to the foreach scene variable, while "source" means the argument variable
            {
                AddForcedNpcsFromLevelParams(builder);
                return;
            }

            try { _levelObject = scene.GetCurrentCustomLevelObject(); } // Store for later use
            catch (InvalidCastException) // Only catch invalid cast exceptions
            {
                CaudexLibPlugin.Log.LogWarning($"SceneObject \"{scene.name}\" contains non-API LevelObjects! Skipping adding Caudex attributes...");
                return;
            }
            catch (Exception e) { throw e; }

            List<NPC> forcedNpcsInclusive = _levelObject.GetForcedNpcsInclusive(false);
            if (forcedNpcsInclusive == null) return;

            for (int i = 0; i < forcedNpcsInclusive.Count; i++)
                builder.ec.npcsToSpawn.Add(forcedNpcsInclusive[i]);
        }

        private static void AddForcedNpcsFromLevelParams(LevelBuilder builder)
        {
            // Use CustomLevelGenerationParameters instead of CustomLevelObject
            List<NPC> forcedNpcsInclusive = ((CustomLevelGenerationParameters)builder.ld).GetForcedNpcsInclusive(false);
            if (forcedNpcsInclusive != null)
                for (int i = 0; i < forcedNpcsInclusive.Count; i++)
                    builder.ec.npcsToSpawn.Add(forcedNpcsInclusive[i]);
        }

        private static void AddPotentialNpcsFromLvl(LevelBuilder builder, SceneObject scene, List<WeightedNPC> npcs)
        {
            if (scene == builder.scene) // "scene" refers to the foreach scene variable, while "source" means the argument variable
            {
                AddPotentialNpcsFromLevelParams((CustomLevelGenerationParameters)builder.ld, npcs);
                return;
            }
            if (!_levelObject) // Use cached CustomLevelObject here instead of doing that again
                return;

            List<WeightedNPC> potentialNpcs = _levelObject.GetPotentialNpcsInclusive(false);
            if (potentialNpcs != null)
                npcs.AddRange(potentialNpcs);
        }

        private static void AddPotentialNpcsFromLevelParams(CustomLevelGenerationParameters lvlParams, List<WeightedNPC> npcs)
        {
            // Use CustomLevelGenerationParameters instead of CustomLevelObject
            List<WeightedNPC> potentialNpcs = lvlParams.GetPotentialNpcsInclusive(false);
            if (potentialNpcs != null)
                npcs.AddRange(potentialNpcs);

            potentialNpcs = lvlParams.GetPotentialNpcs(false);
            if (potentialNpcs != null)
                npcs.AddRange(potentialNpcs);
        }
    }
}
