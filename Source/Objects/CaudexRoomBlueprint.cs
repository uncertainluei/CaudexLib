using System.Collections.Generic;

using BepInEx;
using MTM101BaldAPI;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Objects
{
    public class CaudexRoomBlueprint(PluginInfo plug, string name, RoomCategory cat)
    {
        public CaudexRoomBlueprint(PluginInfo plug, string name, string cat) : this(plug, name, EnumExtensions.ExtendEnum<RoomCategory>(cat))
        {
        }

        public CaudexRoomBlueprint(PluginInfo plug, string name, RoomAsset baseAsset) : this(plug, name, baseAsset.category)
        {
            type = baseAsset.type;
            color = baseAsset.color;

            texFloor = baseAsset.florTex;
            texCeil = baseAsset.ceilTex;
            texWall = baseAsset.wallTex;

            keepTextures = baseAsset.keepTextures;

            lightObj = baseAsset.lightPre;
            mapMaterial = baseAsset.mapMaterial;
            doorMats = baseAsset.doorMats;

            posterChance = baseAsset.posterChance;
            posters = baseAsset.posters;

            windowChance = baseAsset.windowChance;
            windowSet = baseAsset.windowObject;

            itemValMin = baseAsset.minItemValue;
            itemValMax = baseAsset.maxItemValue;

            offLimits = baseAsset.offLimits;
            objectSwaps = baseAsset.basicSwaps;
            forcedItems = baseAsset.itemList;

            functionContainer = baseAsset.roomFunctionContainer;
        }

        public PluginInfo Plugin { get; } = plug;
        public string name = name;
        public RoomCategory category = cat;

        public RoomType type = RoomType.Room;
        public Color color = Color.white;

        public Texture2D texFloor;
        public Texture2D texCeil;
        public Texture2D texWall;
        public bool keepTextures;

        public Transform lightObj;
        public Material mapMaterial;
        public StandardDoorMats doorMats;

        public float posterChance = 0.25f;
        public List<WeightedPosterObject> posters = [];

        public float windowChance;
        public WindowObject windowSet;

        public int itemValMin;
        public int itemValMax = 100;

        public bool offLimits;

        public List<BasicObjectSwapData> objectSwaps = [];
        public List<ItemObject> forcedItems = [];

        public RoomFunctionContainer functionContainer;

        public RoomAsset CreateAsset(string idName)
        {
            RoomAsset roomAsset = RoomAsset.CreateInstance<RoomAsset>();
            ((ScriptableObject)roomAsset).name = $"{type}_{name}_{idName}";
            roomAsset.name = $"{name}_{idName}";

            roomAsset.type = type;
            roomAsset.category = category;
            roomAsset.color = color;

            roomAsset.florTex = texFloor;
            roomAsset.ceilTex = texCeil;
            roomAsset.wallTex = texWall;
            roomAsset.keepTextures = keepTextures;

            roomAsset.lightPre = lightObj;
            roomAsset.mapMaterial = mapMaterial;
            roomAsset.doorMats = doorMats;

            roomAsset.posterChance = posterChance;
            roomAsset.posters = posters;

            roomAsset.windowChance = windowChance;
            roomAsset.windowObject = windowSet;

            roomAsset.minItemValue = itemValMin;
            roomAsset.maxItemValue = itemValMax;

            roomAsset.offLimits = offLimits;

            roomAsset.hasActivity = false;

            roomAsset.basicSwaps = objectSwaps;
            roomAsset.itemList = forcedItems;

            roomAsset.roomFunctionContainer = functionContainer;

            return roomAsset;
        }
    }
}
