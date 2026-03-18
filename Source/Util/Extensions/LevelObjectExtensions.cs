using System.Collections.Generic;
using MTM101BaldAPI;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    /// <summary>
    /// Extensions for <c>CustomLevelObject</c>s and <c>CustomLevelGenerationParameter</c>s
    /// </summary>
    public static class LevelObjectExtensions
    {
        /// <summary>
        /// Returns a list of potential NPCs exclusive to the <c>CustomLevelObject</c>.
        /// These are added to the scene's potential NPC pool if the level is used.
        /// </summary>
        public static List<WeightedNPC> GetPotentialNpcs(this CustomLevelObject lvl)
        {
            object value = lvl.GetCustomModValue(CaudexLibPlugin.ModGuid, "potentialNpcs");
            if (value == null || value is not List<WeightedNPC>)
            {
                value = new List<WeightedNPC>();
                lvl.SetCustomModValue(CaudexLibPlugin.ModGuid, "potentialNpcs", value);
            }

            return (List<WeightedNPC>)value;
        }

        /// <summary>
        /// Returns a list of potential NPCs exclusive to the <c>CustomLevelGenerationParameters</c>.
        /// These are added to the scene's potential NPC pool if the level is used.
        /// </summary>
        public static List<WeightedNPC> GetPotentialNpcs(this CustomLevelGenerationParameters lvl)
        {
            object value = lvl.GetCustomModValue(CaudexLibPlugin.ModGuid, "potentialNpcs");
            if (value == null || value is not List<WeightedNPC>)
            {
                value = new List<WeightedNPC>();
                lvl.SetCustomModValue(CaudexLibPlugin.ModGuid, "potentialNpcs", value);
            }

            return (List<WeightedNPC>)value;
        }
    }
}
