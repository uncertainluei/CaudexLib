using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util
{
    public static class RoomAssetHelper
    {
        public static CellData Cell(int x, int y, int type) => new() { pos = new(x, y), type = type };
        public static List<CellData> CellRect(int width, int height)
        {
            List<CellData> cells = [];
            for (int y = 0; y < height; y++)
            {
                int yOffset = 0;
                if (y == 0)
                    yOffset = 4;
                if (y == height - 1)
                    yOffset += 1;

                for (int x = 0; x < width; x++)
                {
                    int type = yOffset;
                    if (x == 0)
                        type += 8;
                    if (x == width - 1)
                        type += 2;

                    cells.Add(Cell(x, y, type));
                }
            }
            return cells;
        }
        public static PosterData PosterData(int x, int y, PosterObject pst, Direction dir) => new() { position = new IntVector2(x, y), poster = pst, direction = dir };
        public static BasicObjectData ObjectPlacement(Component obj, Vector3 pos, Vector3 eulerAngles) => new() { position = pos, prefab = obj.transform, rotation = Quaternion.Euler(eulerAngles) };
        public static BasicObjectData ObjectPlacement(Component obj, Vector3 pos, float angle) => ObjectPlacement(obj, pos, Vector3.up * angle);
    }
}
