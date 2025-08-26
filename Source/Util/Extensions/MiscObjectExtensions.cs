using UnityCipher;
using UnityEngine;

namespace UncertainLuei.CaudexLib.Util.Extensions
{
    public static class MiscObjectExtensions
    {
        public static void SetTextures(this SodaMachine vending, Texture2D normal, Texture2D empty)
        {
            Renderer renderer = vending.meshRenderer;
            renderer.sharedMaterials =
            [
                renderer.sharedMaterials[0],
                new(renderer.sharedMaterials[1])
                {
                    name = normal.name,
                    mainTexture = normal
                }
            ];
            vending.outOfStockMat = new(vending.outOfStockMat)
            {
                name = empty.name,
                mainTexture = empty
            };
        }
    }
}
