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
    static class LevelGeneratorPatches
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
                CaudexLibPlugin.Log.LogError("Transpiler \"CaudexLib.LevelGeneratorPatches.EventTranspiler\" wasn't properly applied!");

            yield break;
        }

        // If a larger room won't work, then keep trying until there are none left
        private static bool BruteForceRoomPlacements(LevelGenerator gen, WeightedSelection<RoomAsset>[] rooms, Random rng, bool addDoor, out RoomController result)
        {
            List<WeightedSelection<RoomAsset>> roomsAsList = new(rooms);
            int i;
            while (roomsAsList.Count > 0)
            {
                i = WeightedSelection<RoomAsset>.ControlledRandomIndexList(roomsAsList, rng);
                if (gen.RandomlyPlaceRoom(roomsAsList[i].selection, addDoor, out result))
                    return true;
                    
                roomsAsList.RemoveAt(i);
            }
            CaudexLibPlugin.Log.LogWarning("Failed to spawn any room assets!");

            result = null;
            return false;
        }

        private static readonly MethodInfo bruteForcePlacements = AccessTools.Method(typeof(LevelGeneratorPatches), "BruteForceRoomPlacements");
        private static readonly object controlledRng = AccessTools.Field(typeof(LevelBuilder), "controlledRNG");
        private static readonly object randomSelection = AccessTools.Method(typeof(WeightedSelection<RoomAsset>), "ControlledRandomSelection");

        // If there are any BepInEx/Harmony professionals who know how to check for out parameters any other way, then tell me ASAP.
        private static readonly object randomlyPlace = AccessTools.FirstMethod(typeof(LevelGenerator), (method) =>
        {
            if (method.Name != "RandomlyPlaceRoom")
                return false;

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 3 && parameters[2].IsOut;
        });

        [HarmonyPatch("Generate", MethodType.Enumerator), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RoomPlacementTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            byte patchesDone = 0;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length, i = 0;

            for (; i < length; i++)
            {
                // if (levelGenerator.RandomlyPlaceRoom(WeightedSelection<RoomAsset>.ControlledRandomSelection(potentialRooms, levelGenerator.controlledRNG), addDoor: true, out newRoom))
                if (i+6 < length &&
                    array[i].opcode   == OpCodes.Ldloc_S    &&
                    array[i+1].opcode == OpCodes.Ldloc_2    &&
                    array[i+2].opcode == OpCodes.Ldfld      &&
                    array[i+2].operand == controlledRng     &&
                    array[i+3].opcode == OpCodes.Call       &&
                    array[i+3].operand == randomSelection   &&
                    array[i+4].opcode == OpCodes.Ldc_I4_1   &&
                    array[i+5].opcode == OpCodes.Ldloca_S   &&
                    array[i+6].opcode == OpCodes.Call       &&
                    array[i+6].operand == randomlyPlace     &&
                    array[i+7].opcode == OpCodes.Brfalse)
                {
                    patchesDone++;
                    yield return array[i];
                    yield return array[i+1];
                    yield return array[i+2];
                    yield return array[i+4];
                    yield return array[i+5];
                    yield return new CodeInstruction(OpCodes.Call, bruteForcePlacements);
                    i += 7;
                }
                yield return array[i];
            }

            if (patchesDone == 0)
                CaudexLibPlugin.Log.LogError("No patches have been done for \"CaudexLib.LevelGeneratorPatches.RoomPlacementTranspiler\"!");

            yield break;
        }
    }
}
