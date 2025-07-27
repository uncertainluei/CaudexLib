namespace UncertainLuei.CaudexLib.Objects
{
    public class CaudexItemObject : ItemObject
    {
        internal static void DefaultOnUseSuccess(ItemManager im)
        {
            im.RemoveItem(im.selectedItem);
            if (CoreGameManager.Instance.inventoryChallenge)
                im.RemoveItemSlot(im.selectedItem);
        }
        public virtual void OnUseSuccess(ItemManager im) => DefaultOnUseSuccess(im);
    }

    public class CaudexMultiItemObject : CaudexItemObject
    {
        public ItemObject nextStage;
        public override void OnUseSuccess(ItemManager im)
        {
            if (nextStage == null)
            {
                im.SetItem(nextStage, im.selectedItem);
                return;
            }
            base.OnUseSuccess(im);
        }
    }
}
