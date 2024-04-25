using SDPublicFramework;
using UnityEngine;
using UnityModManagerNet;

namespace CatsItems
{
    public class CatItemsSettings : UnityModManager.ModSettings, IDrawable
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<CatItemsSettings>(this, modEntry);
        }

        public void OnChange()
        {
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            _changingGogglesKey = GUILayout.Toggle(_changingGogglesKey, "Change the refined goggles light toggle key.", new GUILayoutOption[0]);
            if (_changingGogglesKey)
            {
                GUILayout.Label("Press the button that will be your new light toggle key (Esc to cancel).");
                ToggleGogglesKey = CatUtility.ChooseNewKey(ref _changingGogglesKey, ToggleGogglesKey);
            }
            GUILayout.Label("Current refined goggles light toggle key: " + ToggleGogglesKey);
        }

        public KeyCode ToggleGogglesKey = KeyCode.X;
        private static bool _changingGogglesKey = false;
    }

}
