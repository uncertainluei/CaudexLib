using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.CaudexLib.Objects
{
    public class CaudexItemObject : ItemObject
    {
        public virtual string LocalizedName => UnlocalizedName.Localize();
        public string UnlocalizedName => this.GetName(false);
        public virtual string NameKey => nameKey;

        public virtual string LocalizedDesc => UnlocalizedDesc.Localize();
        public string UnlocalizedDesc => this.GetDescription(false);
        public virtual string DescKey => descKey;

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
        public override string LocalizedName => string.Format("Itm_CaudexLib_MultiItemFormat".Localize(), base.LocalizedName, stateNo);

        public byte stateNo;
        public ItemObject nextStage;

        public override void OnUseSuccess(ItemManager im)
        {
            if (nextStage != null)
            {
                im.SetItem(nextStage, im.selectedItem);
                return;
            }
            base.OnUseSuccess(im);
        }
    }
}
