using System;
using System.Linq;
using UnityEngine;
using UnityModManagerNet;

namespace IslandMakerTools
{
    public class IMTSettings : UnityModManager.ModSettings, IDrawable
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<IMTSettings>(this, modEntry);
        }

        public void OnChange()
        {
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            DrawGUI(() => { ScaleOnAllSides = GUILayout.Toggle(ScaleOnAllSides, "Scale objects on all sides", new GUILayoutOption[0]); });

            if (_changingShortcutScale)
            {
                DrawGUI(() => { GUILayout.Label("Enter new shortcut key for quickly selecting scaling mode... (Esc to cancel)."); });
                ShortcutScale = ChooseNewKey(ref _changingShortcutScale, ShortcutScale);
            }
            else
            {
                DrawGUI(() => { _changingShortcutScale = GUILayout.Toggle(_changingShortcutScale, $"Change quick-select scale mode [{ShortcutScale}]", new GUILayoutOption[0]); });
            }
        }

        public void DrawGUI(Action action)
        {
            GUILayout.BeginHorizontal();
            action();
            GUILayout.EndHorizontal();
        }

        private static KeyCode ChooseNewKey(ref bool changing, KeyCode original)
        {
            var allKeys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();

            foreach (var key in allKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    changing = false;

                    if (key == KeyCode.Escape) return original;
                    return key;
                }
            }
            return original;
        }

        public bool ScaleOnAllSides = false;
        public KeyCode ShortcutScale = KeyCode.Y;
        private static bool _changingShortcutScale = false;
    }
}
