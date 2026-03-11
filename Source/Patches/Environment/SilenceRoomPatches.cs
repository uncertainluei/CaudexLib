using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

using UncertainLuei.CaudexLib.Components;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Patches
{
    [HarmonyPatch(typeof(SilenceRoomFunction))]
    static class SilenceRoomPatches
    {
        [HarmonyPatch("OnDestroy"), HarmonyPrefix]
        private static bool OnDestroy(SilenceRoomFunction __instance)
        {
            // Skip if room is null or no players are in the room
            if (!__instance.room || __instance.playersInRoom == 0)
                return false;

            PlayerManager player;
            for (int i = 0; i < CoreGameManager.Instance.TotalPlayers; i++)
            {
                player = CoreGameManager.Instance.GetPlayer(i);
                if (player && player.currentRoom == __instance.room)
                    __instance.OnPlayerExit(player);
            }
            return false;
        }

        [HarmonyPatch("OnGenerationFinished"), HarmonyPatch("OnPlayerEnter"), HarmonyPatch("OnPlayerExit"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> WipeAllButBaseCall(IEnumerable<CodeInstruction> instructions)
        {
            bool patched = false;

            CodeInstruction[] array = instructions.ToArray();
            int length = array.Length;

            for (int i = 0; i < length; i++)
            {
                yield return array[i];
                if (array[i].opcode != OpCodes.Call) continue;

                patched = true;
                yield return array[length-1];
                break;
            }

            if (!patched)
                CaudexLibPlugin.Log.LogError("Transpiler \"CaudexLib.SilenceRoomPatches.WipeAllButBaseCall\" wasn't properly applied!");

            yield break;
        }

        [HarmonyPatch("OnGenerationFinished"), HarmonyPostfix]
        private static void OnGenerationFinished(SilenceRoomFunction __instance)
        {
            // ACTUALLY account the silence cells field
            if (!__instance.silenceCells) return;

            foreach (Cell cell in __instance.room.cells)
                cell.SetSilence(value: true);
        }

        [HarmonyPatch("OnPlayerEnter"), HarmonyPostfix]
        private static void OnPlayerEnter(PlayerManager player) => player.GetComponent<PlayerSilenceManager>().Silence(true);

        [HarmonyPatch("OnPlayerExit"), HarmonyPostfix]
        private static void OnPlayerExit(PlayerManager player) => player.GetComponent<PlayerSilenceManager>().Silence(false);
    }
}
