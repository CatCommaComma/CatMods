using Beam;
using Beam.Crafting;
using System.Collections.Generic;

namespace SDPublicFramework
{
    public class InteractiveObject_Recipe : InteractiveObject
    {
        public List<string> UnlockedRecipeNames { get { return _unlockedRecipeNames; } set { _unlockedRecipeNames = value; } }

        public override void Use()
        {
            base.Use();
            UnlockRecipes();
        }

        private void UnlockRecipes()
        {
            bool learned = false;

            foreach (string recipe in _unlockedRecipeNames)
            {
                CraftingCombination combination = Owner.Crafter.CraftingCombinations.GetCombination(recipe);
                if (combination != null && !combination.Unlocked)
                {
                    combination.Unlocked = true;
                    learned = true;
                }
            }

            if (learned) CatUtility.PopNotification("Reading this recipe thought you some new crafting combinations!", 3f);
            if (!learned) CatUtility.PopNotification("You already have all knowledge in this recipe...", 2.8f);
        }

        private List<string> _unlockedRecipeNames;
    }
}
