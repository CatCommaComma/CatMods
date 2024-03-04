namespace SDPublicFramework
{
    public interface IEquippable
    {
        bool IsEquipped { get; }
        EquippableSystem.EquippableArea EquippableArea { get; }
        void OnEquip(bool initial = false);
        void OnUnequip();
    }
}
