using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;

using UncertainLuei.CaudexLib.Objects;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class ItemBuilderExtensions
    {
        // Taken wholesale from the MTM API as I sure as heck did not want to do a reverse patch
        public static I Build<I>(this ItemBuilder builder) where I : CaudexItemObject
        {
            I itmObj = ScriptableObject.CreateInstance<I>();
            itmObj.nameKey = builder.localizedText;
            itmObj.descKey = builder.localizedDescription;

            itmObj.name = builder.localizedText;
            itmObj.itemType = builder.itemEnum;
            if (builder.itemEnumName != "")
                itmObj.itemType = EnumExtensions.ExtendEnum<Items>(builder.itemEnumName);

            itmObj.itemSpriteSmall = builder.smallSprite;
            itmObj.itemSpriteLarge = builder.largeSprite;
            itmObj.price = builder.price;
            itmObj.value = builder.generatorCost;
            itmObj.overrideDisabled = builder.overrideDisabled;

            if (builder.itemObjectType != null)
            {
                GameObject obj = new();
                obj.SetActive(false);
                Item comp = (Item)obj.AddComponent(builder.itemObjectType);
                comp.name = "Obj" + itmObj.name;
                itmObj.item = comp;
                obj.ConvertToPrefab(true);
            }

            if (builder.objectReference != null)
                itmObj.item = builder.objectReference;

            if (builder.instantUse)
            {
                builder.flags |= ItemFlags.InstantUse;
                itmObj.addToInventory = false;
            }

            itmObj.audPickupOverride = builder.pickupSoundOverride;

            if (builder.metaDataToAddTo != null)
            {
                builder.metaDataToAddTo.itemObjects = builder.metaDataToAddTo.itemObjects.AddToArray(itmObj);
                itmObj.AddMeta(builder.metaDataToAddTo);
                return itmObj;
            }
            ItemMetaData itemMeta = new(builder.info, itmObj);
            itemMeta.tags.AddRange(builder.tags);
            itemMeta.flags = builder.flags;
            itmObj.AddMeta(itemMeta);
            return itmObj;
        }

        public static CaudexMultiItemObject BuildAsMulti(this ItemBuilder builder, byte stateCount)
        {
            CaudexMultiItemObject itmObj = builder.Build<CaudexMultiItemObject>();

            ItemMetaData meta = itmObj.GetMeta();
            meta.flags |= ItemFlags.MultipleUse;
            
            CaudexMultiItemObject last = null;
            string objName = itmObj.name;
            itmObj.stateNo = 0;

            for (byte i = 0; ; i++)
            {
                itmObj.stateNo++;
                itmObj.name = $"{objName}_{itmObj.stateNo}";
                itmObj.nextStage = last;

                CaudexLibPlugin.Log.LogWarning(i == stateCount - 1);
                if (i == stateCount - 1) break;
                CaudexLibPlugin.Log.LogWarning("Created new");

                last = itmObj;
                itmObj = ScriptableObject.Instantiate(itmObj);
                meta.itemObjects = meta.itemObjects.AddToArray(itmObj);
                itmObj.AddMeta(meta);
            }
            return itmObj;
        }
    }
}
