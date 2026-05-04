using System.Linq;
using UnityCipher;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class RoomFunctionExtensions
    {
        public static T AddFunction<T>(this RoomFunctionContainer container) where T : RoomFunction
        {
            T newFunc = container.gameObject.AddComponent<T>();
            container.AddFunction(newFunc);
            return newFunc;
        }

        public static void RemoveFunction<T>(this RoomFunctionContainer container, bool deleteComponent = true) where T : RoomFunction
        {
            RoomFunction function = container.functions.FirstOrDefault(x => x is T);
            if (function == null) return;

            container.functions.Remove(function);
            if (deleteComponent)
                GameObject.DestroyImmediate(function);
        }

        public static DoorAssignerRoomFunction AddDoorAssigner(this RoomFunctionContainer container, Door doorPrefab)
        {
            DoorAssignerRoomFunction doorAssigner = container.AddFunction<DoorAssignerRoomFunction>();
            doorAssigner.doorPre = doorPrefab;
            return doorAssigner;
        }

        public static SpecialRoomSwingingDoorsBuilder AddSpecialRoomDoors(this RoomFunctionContainer container, Door doorPrefab)
        {
            SpecialRoomSwingingDoorsBuilder specialDoorBuilder = container.AddFunction<SpecialRoomSwingingDoorsBuilder>();
            specialDoorBuilder.swingDoorPre = doorPrefab;
            return specialDoorBuilder;
        }

        public static ChalkboardBuilderFunction AddChalkboardBuilder(this RoomFunctionContainer container, params WeightedPosterObject[] posters)
        {
            ChalkboardBuilderFunction chalkBuilder = container.AddFunction<ChalkboardBuilderFunction>();
            chalkBuilder.chalkBoards = posters;
            return chalkBuilder;
        }

        public static ChalkboardBuilderFunction AddChalkboardBuilder(this RoomFunctionContainer container, PosterObject poster)
            => AddChalkboardBuilder(container, [poster.Weighted(100)]);

        public static void AddBulkChalkboardBuilder(this RoomFunctionContainer container, PosterObject[] posters)
        {
            foreach (PosterObject pst in posters)
                AddChalkboardBuilder(container, pst);
        }

        public static SunlightRoomFunction AddSunlight(this RoomFunctionContainer container, Color lightColor)
        {
            SunlightRoomFunction sunlightRoom = container.AddFunction<SunlightRoomFunction>();
            sunlightRoom.color = lightColor;
            return sunlightRoom;
        }
    }
}
