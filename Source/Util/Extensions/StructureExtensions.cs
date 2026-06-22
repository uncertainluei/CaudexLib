using System.Collections.Generic;
using System.Linq;

using MTM101BaldAPI;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class StructureHelperExtensions
    {
        public static StructureWithParameters WithParameters(this StructureBuilder builder, StructureParameters parameters)
            => new() { prefab = builder, parameters = parameters };

        public static StructureWithParameters WithParameters(this StructureBuilder builder, float[] chances = null, IntVector2[] minMaxes = null, WeightedGameObject[] prefabs = null)
        {
            StructureParameters parameters = new();
            if (chances != null)
                parameters.chance = chances;
            if (minMaxes != null)
                parameters.minMax = minMaxes;
            if (prefabs != null)
                parameters.prefab = prefabs;
            return builder.WithParameters(parameters);
        }
    }
}
