using System;
using System.Collections.Generic;
using BepInEx;
using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Objects;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class ItemObjectExtensions
    {
        public static void AddNameOverride(this ItemMetaData itm, Func<ItemObject, bool, string> stringOverride)
            => AddStringOverride(itm, stringOverride, ref nameOverrides);
        public static void AddDescOverride(this ItemMetaData itm, Func<ItemObject, bool, string> stringOverride)
            => AddStringOverride(itm, stringOverride, ref descOverrides);

        private static void AddStringOverride(ItemMetaData itm, Func<ItemObject, bool, string> stringOverride, ref Dictionary<ItemMetaData, List<Func<ItemObject, bool, string>>> dict)
        {
            if (!dict.ContainsKey(itm))
                dict.Add(itm, []);

            dict[itm].Add(stringOverride);
        }

        private static Dictionary<ItemMetaData, List<Func<ItemObject, bool, string>>> nameOverrides = [];
        private static Dictionary<ItemMetaData, List<Func<ItemObject, bool, string>>> descOverrides = [];

        private static bool TryGetOverride(this ItemObject itm, ref Dictionary<ItemMetaData, List<Func<ItemObject, bool, string>>> dict, bool localized, out string newVal)
        {
            newVal = null;
            ItemMetaData meta = itm.GetMeta();

            if (dict == null ||
                meta == null ||
                !dict.ContainsKey(meta))
                return false;

            for (int i = dict[meta].Count-1; i >= 0; i++)
            {
                string val = dict[meta][i](itm, localized);
                if (val.IsNullOrWhiteSpace()) continue;
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
            if (itm is CaudexItemObject cItm)
            {
                cItm.OnUseSuccess(im);
                return;
            }
            CaudexItemObject.DefaultOnUseSuccess(im);
        }

        public static void SetFunctionAcrossMeta(this ItemObject itm, Item func)
        {
            foreach (ItemObject itmObj in itm.GetMeta().itemObjects)
                itmObj.item = func;
        }
    }
}
