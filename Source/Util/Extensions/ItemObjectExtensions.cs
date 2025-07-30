using System;
using System.Collections.Generic;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Objects;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class ItemObjectExtensions
    {
        public delegate bool ItemStringOverride(ItemObject itm, bool localized, out string newVal);

        private static void AddNameOverride(ItemMetaData itm, ItemStringOverride stringOverride, ref Dictionary<ItemMetaData, List<ItemStringOverride>> dict)
        {
            if (!dict.ContainsKey(itm))
                dict.Add(itm, []);

            dict[itm].Add(stringOverride);
        }

        private static Dictionary<ItemMetaData, List<ItemStringOverride>> nameOverrides = [];
        private static Dictionary<ItemMetaData, List<ItemStringOverride>> descOverrides = [];

        private static bool TryGetOverride(this ItemObject itm, ref Dictionary<ItemMetaData, List<ItemStringOverride>> dict, bool localized, out string newVal)
        {
            newVal = null;
            ItemMetaData meta = itm.GetMeta();

            if (dict == null ||
                meta == null ||
                !dict.ContainsKey(meta))
                return false;

            for (int i = dict[meta].Count-1; i >= 0; i++)
            {
                if (!dict[meta][i](itm, localized, out string val)) continue;
                newVal = val;
                return true;
            }
            return false;
        }

        public static string GetName(this ItemObject itm, bool localized = true)
        {
            if (itm is CaudexItemObject cItm)
                return localized ? cItm.LocalizedName : cItm.UnlocalizedName;
            if (itm.TryGetOverride(ref nameOverrides, localized, out string newVal))
                return newVal;

            string name = itm.nameKey;
            if (localized)
                name = name.Localize();
            return name;
        }

        public static string GetDescription(this ItemObject itm, bool localized = true)
        {
            if (itm is CaudexItemObject cItm)
                return localized ? cItm.LocalizedDesc : cItm.UnlocalizedDesc;
            if (itm.TryGetOverride(ref descOverrides, localized, out string newVal))
                return newVal;

            string desc = itm.descKey;
            if (localized)
                desc = desc.Localize();
            return desc;
        }

        public static void OnUseSuccess(this ItemObject itm, ItemManager im)
        {
            CaudexLibPlugin.Log.LogError("Succesfully Overridden!!");
            if (itm is CaudexItemObject cItm)
            {
                cItm.OnUseSuccess(im);
                return;
            }
            CaudexItemObject.DefaultOnUseSuccess(im);
        }
    }
}
