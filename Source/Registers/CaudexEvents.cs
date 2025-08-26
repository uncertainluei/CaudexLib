using System;
using System.Collections.Generic;
using System.Text;

namespace UncertainLuei.CaudexLib.Registers
{
    public static class CaudexEvents
    {
        public delegate void ItemUseEvent(ItemManager im, ItemObject itm);
        public static event ItemUseEvent OnItemUse;

        internal static void ItemUsed(ItemManager im, ItemObject itm)
            => OnItemUse?.Invoke(im, itm);

        public static event Action OnNotebookCollect;

        internal static void NotebookCollected()
            => OnNotebookCollect?.Invoke();

    }
}
