using System.Collections.Generic;
using MTM101BaldAPI;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    /// <summary>
    /// Extensions for <c>CustomLevelObject</c>s and <c>CustomLevelGenerationParameter</c>s
    /// </summary>
    public static class LevelObjectExtensions
    {
        internal static List<T> GetModListValue<T>(CustomLevelObject lvl, string key, bool createIfMissing = true)
        {
            object value = lvl.GetCustomModValue(CaudexLibPlugin.ModGuid, key);
            if (value != null && value is List<T> list)
                return list;

            if (!createIfMissing) return null;

            value = new List<T>();
            lvl.SetCustomModValue(CaudexLibPlugin.ModGuid, key, value);
            return (List<T>)value;
        }

        // MTM, PLEASE have some unified system that doesn't rely on duplicated code, PLEEAAASSSEEEE
        internal static List<T> GetModListValue<T>(CustomLevelGenerationParameters lvl, string key, bool createIfMissing = true)
        {
            object value = lvl.GetCustomModValue(CaudexLibPlugin.ModGuid, key);
            if (value != null && value is List<T> list)
                return list;

            if (!createIfMissing) return null;

            value = new List<T>();
            lvl.SetCustomModValue(CaudexLibPlugin.ModGuid, key, value);
            return (List<T>)value;
        }

        /// <summary>
        /// Returns a list of potential NPCs exclusive to the <c>CustomLevelObject</c>.
        /// These are added to the scene's potential NPC pool if the level is used.
        /// </summary>
        public static List<WeightedNPC> GetPotentialNpcs(this CustomLevelObject lvl, bool createIfMissing = true)
            => GetModListValue<WeightedNPC>(lvl, "potentialNpcs", createIfMissing);

        /// <summary>
        /// Returns a list of potential NPCs in the <c>CustomLevelObject</c> that'll be added in all subsquent levels.
        /// </summary>
        public static List<WeightedNPC> GetPotentialNpcsInclusive(this CustomLevelObject lvl, bool createIfMissing = true)
            => GetModListValue<WeightedNPC>(lvl, "potentialNpcsInclusive", createIfMissing);

        /// <summary>
        /// Returns a list of forced NPCs in the <c>CustomLevelObject</c> that'll be added in all subsquent levels.
        /// Abides by the same logic as forcedNPCs in <c>SceneObject</c>s.
        /// </summary>
        public static List<NPC> GetForcedNpcsInclusive(this CustomLevelObject lvl, bool createIfMissing = true)
            => GetModListValue<NPC>(lvl, "forcedNpcsInclusive", createIfMissing);


        /// <summary>
        /// Returns a list of potential NPCs exclusive to the <c>CustomLevelGenerationParameters</c>.
        /// These are added to the scene's potential NPC pool if the level is used.
        /// </summary>
        public static List<WeightedNPC> GetPotentialNpcs(this CustomLevelGenerationParameters lvlParams, bool createIfMissing = true)
            => GetModListValue<WeightedNPC>(lvlParams, "potentialNpcs", createIfMissing);

        /// <summary>
        /// Returns a list of potential NPCs in the <c>CustomLevelGenerationParameters</c> that'll be added in all subsquent levels.
        /// </summary>
        public static List<WeightedNPC> GetPotentialNpcsInclusive(this CustomLevelGenerationParameters lvlParams, bool createIfMissing = true)
            => GetModListValue<WeightedNPC>(lvlParams, "potentialNpcsInclusive", createIfMissing);

        /// <summary>
        /// Returns a list of forced NPCs in the <c>CustomLevelObject</c> that'll be added in all subsquent levels.
        /// Abides by the same logic as forcedNPCs in <c>SceneObject</c>s.
        /// </summary>
        public static List<NPC> GetForcedNpcsInclusive(this CustomLevelGenerationParameters lvlParams, bool createIfMissing = true)
            => GetModListValue<NPC>(lvlParams, "forcedNpcsInclusive", createIfMissing);
    }
}
