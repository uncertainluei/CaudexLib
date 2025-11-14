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

        public virtual bool OverrideUseResult(ItemManager im) => false;
    }

    public class CaudexMultiItemObject : CaudexItemObject
    {
        public override string LocalizedName => string.Format("Itm_CaudexLib_MultiItemFormat".Localize(), base.LocalizedName, stateNo);

        public byte stateNo;
        public ItemObject nextStage;

        public override bool OverrideUseResult(ItemManager im)
        {
            if (nextStage)
            {
                im.SetItem(nextStage, im.selectedItem);
                return true;
            }
            return base.OverrideUseResult(im);
        }
    }
}
