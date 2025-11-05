using System.Collections.Generic;
using System.Linq;

using MTM101BaldAPI;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class WeightedHelperExtensions
    {
        public static T Weighted<X, T>(this X selection, int weight) where T : WeightedSelection<X>, new()
            => new() { selection = selection, weight = weight };
        public static WeightedNPC Weighted(this NPC selection, int weight) => selection.Weighted<NPC, WeightedNPC>(weight);
        public static WeightedItemObject Weighted(this ItemObject selection, int weight) => selection.Weighted<ItemObject, WeightedItemObject>(weight);
        public static WeightedPosterObject Weighted(this PosterObject selection, int weight) => selection.Weighted<PosterObject, WeightedPosterObject>(weight);
        public static WeightedRoomAsset Weighted(this RoomAsset selection, int weight) => selection.Weighted<RoomAsset, WeightedRoomAsset>(weight);
        public static WeightedGameObject Weighted(this GameObject selection, int weight) => selection.Weighted<GameObject, WeightedGameObject>(weight);
        public static WeightedTransform Weighted(this Transform selection, int weight) => selection.Weighted<Transform, WeightedTransform>(weight);

        // Done for future-proofing
        public static int GetNpcWeight(this ICollection<WeightedNPC> collection, Character characterToCopy, int fallback = -1)
        {
            if (collection.Count > 0)
            {
                WeightedNPC weightedToCopy = collection.FirstOrDefault(x => x != null && x.selection?.character == characterToCopy);
                if (weightedToCopy != null)
                    return weightedToCopy.weight;
            }

            CaudexLibPlugin.Log.LogWarning($"No character with enum {characterToCopy.ToStringExtended()} exists in the collection!");
            return fallback;
        }
        public static void CopyNpcWeight(this List<WeightedNPC> list, Character characterToCopy, NPC newNpc)
        {
            int weight = list.GetNpcWeight(characterToCopy);
            if (weight > 0)
                list.Add(newNpc.Weighted(weight));
        }

        // Apparently there are TWO weighted classes that hold an ItemObject. Froge Mod Loader.
        public static int GetItemWeight<T>(this ICollection<T> collection, Items itmToCopy, int fallback = -1) where T : WeightedSelection<ItemObject>
        {
            if (collection.Count > 0)
            {
                T weightedToCopy = collection.FirstOrDefault(x => x != null && x.selection?.itemType == itmToCopy);
                if (weightedToCopy != null)
                    return weightedToCopy.weight;
            }

            CaudexLibPlugin.Log.LogWarning($"No item with type enum {itmToCopy.ToStringExtended()} exists in the collection!");
            return fallback;
        }
        public static void CopyItemWeight(this List<WeightedItemObject> list, Items itmToCopy, ItemObject newItm)
        {
            int weight = list.GetItemWeight(itmToCopy);
            if (weight > 0)
                list.Add(newItm.Weighted(weight));
        }
    }
}
