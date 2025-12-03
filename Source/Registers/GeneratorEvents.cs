using System;
using System.Collections.Generic;
using BepInEx;
using MTM101BaldAPI;

namespace UncertainLuei.CaudexLib.Registers
{
    public enum CaudexGeneratorEventType
    {
        /// <summary>
        /// Runs when the generator starts.
        /// </summary>
        Start, // runs when generation starts.

        /// <summary>
        /// Runs after all additional and forced NPCs are added, but before their spawning positions are set.
        /// Ideal for spawning NPCs with different spawning criteria or adding items/posters to generation based on an NPC's presence. 
        /// </summary>
        NpcPrep, // runs after getting NPCs from levelobject

        /// <summary>
        /// Runs when all generator actions.
        /// </summary>
        Finalizer // runs when the generator is pretty much complete
    }

    public static class CaudexGeneratorEvents
    {
        public delegate void GenerationEventAction(LevelGenerator gen);
        private static readonly List<GenerationEventAction>[] generationEventActions = new List<GenerationEventAction>[(byte)CaudexGeneratorEventType.Finalizer+1];

        public static void AddAction(CaudexGeneratorEventType type, GenerationEventAction action)
        {
            if (generationEventActions[(byte)type] == null)
                generationEventActions[(byte)type] = [];
            generationEventActions[(byte)type].Add(action);
        }

        public static void RemoveAction(CaudexGeneratorEventType type, GenerationEventAction action)
            => generationEventActions[(byte)type].Remove(action);

        internal static void Invoke(LevelGenerator gen, CaudexGeneratorEventType type)
        {
            List<GenerationEventAction> actions = generationEventActions[(byte)type];
            if (actions == null || actions.Count == 0)
                return;
                
            for (int i = 0; i < actions.Count; i++)
            {
                try
                {
                    actions[i]?.Invoke(gen);
                }
                catch (Exception e)
                {
                    CaudexLibPlugin.Log.LogError(e);
                }
            }
        }
    }
}