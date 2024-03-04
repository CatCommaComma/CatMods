using Beam.Crafting;

namespace SDPublicFramework
{
    public class ModdedCraftingCombo
    {
        public ModdedCraftingCombo(string category, string subcategory, CraftingCombination combination)
        {
            Category = category;
            Subcategory = subcategory;
            Combination = combination;
        }

        public byte InsertPlace;
        public string Category;
        public string Subcategory;
        public CraftingCombination Combination;
    }
}