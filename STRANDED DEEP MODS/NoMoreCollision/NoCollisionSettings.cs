using UnityModManagerNet;
using UnityEngine;
using System;
using System.Linq;

namespace NoMoreCollision
{
    public class NoCollisionSettings : UnityModManager.ModSettings, IDrawable
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<NoCollisionSettings>(this, modEntry);
        }

        public void OnChange()
        {
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            BuildAnywhere = GUILayout.Toggle(BuildAnywhere, "Build anywhere mode (unstable)", new GUILayoutOption[0]);
            if (BuildAnywhere)
            {
                GUILayout.Label("==============================================================");
                AllowSnapping = GUILayout.Toggle(AllowSnapping, "Allow snapping", new GUILayoutOption[0]);
                KeepLastInfo = GUILayout.Toggle(KeepLastInfo, "Keep last structure's info (rotation and scale) for next structure", new GUILayoutOption[0]);
                GUILayout.Label("--------------------------------------------------------------");

                if (Main.ChangingBuildAnywhereKey)
                {
                    GUILayout.Label("Enter new shortcut key for toggling build anywhere mode... (Esc to cancel).");
                    ShortcutBuildAnywhere = ChooseNewKey(ref Main.ChangingBuildAnywhereKey, ShortcutBuildAnywhere);
                }
                else
                {
                    Main.ChangingBuildAnywhereKey = GUILayout.Toggle(Main.ChangingBuildAnywhereKey, $"Change build anywhere toggle shortcut [{ShortcutBuildAnywhere}]", new GUILayoutOption[0]);
                }

                if (Main.ChangingModeKey)
                {
                    GUILayout.Label("Enter new shortcut key for switching structure edit modes... (Esc to cancel).");
                    ShortcutChangeMode = ChooseNewKey(ref Main.ChangingModeKey, ShortcutChangeMode);
                }
                else
                {
                    Main.ChangingModeKey = GUILayout.Toggle(Main.ChangingModeKey, $"Change switch structure edit mode shortcut [{ShortcutChangeMode}]", new GUILayoutOption[0]);
                }

                if (Main.ChangingCicleAxisKey)
                {
                    GUILayout.Label("Enter new shortcut key for switching between different target axis... (Esc to cancel).");
                    CycleAxis = ChooseNewKey(ref Main.ChangingCicleAxisKey, CycleAxis);
                }
                else
                {
                    Main.ChangingCicleAxisKey = GUILayout.Toggle(Main.ChangingCicleAxisKey, $"Change switch between different target axis shortcut [{CycleAxis}]", new GUILayoutOption[0]);
                }

                if (Main.ChangingSnappingKey)
                {
                    GUILayout.Label("Enter new shortcut key for toggling snapping... (Esc to cancel).");
                    ShortcutAllowSnapping = ChooseNewKey(ref Main.ChangingSnappingKey, ShortcutAllowSnapping);
                }
                else
                {
                    Main.ChangingSnappingKey = GUILayout.Toggle(Main.ChangingSnappingKey, $"Change snapping toggle shortcut [{ShortcutAllowSnapping}]", new GUILayoutOption[0]);
                }

                if (Main.ChangingUnstuckKey)
                {
                    GUILayout.Label("Enter new unstuck shortcut... (Esc to cancel).");
                    ShortcutUnstuck = ChooseNewKey(ref Main.ChangingUnstuckKey, ShortcutUnstuck);
                }
                else
                {
                    Main.ChangingUnstuckKey = GUILayout.Toggle(Main.ChangingUnstuckKey, $"Change unstuck shortcut [{ShortcutUnstuck}]", new GUILayoutOption[0]);
                }

                if (Main.ChangingResetKey)
                {
                    GUILayout.Label("Enter new shortcut key for resetting current structure edit mode axis... (Esc to cancel).");
                    ResetCurrentMode = ChooseNewKey(ref Main.ChangingResetKey, ResetCurrentMode);
                }
                else
                {
                    Main.ChangingResetKey = GUILayout.Toggle(Main.ChangingResetKey, $"Change reset current structure edit mode axis shortcut [{ResetCurrentMode}]", new GUILayoutOption[0]);
                }

                if (Main.ChangingKeepLastInfoKey)
                {
                    GUILayout.Label("Enter new shortcut key for toggling keep last construction's info... (Esc to cancel).");
                    ShortcutKeepLastInfo = ChooseNewKey(ref Main.ChangingKeepLastInfoKey, ShortcutKeepLastInfo);
                }
                else
                {
                    Main.ChangingKeepLastInfoKey = GUILayout.Toggle(Main.ChangingKeepLastInfoKey, $"Change keep last construction's info toggle shortcut [{ShortcutKeepLastInfo}]", new GUILayoutOption[0]);
                }

                GUILayout.Label("--------------------------------------------------------------");
                GUILayout.Label("Rotation - X rotating around horizontal axis, Y rotating around vertical axis, Z rotating around axis going towards you");
                GUILayout.Label("Scaling works by stretching/narrowing those same axises");
                GUILayout.Label("Scaling is always relative to its rotation, rotation is always relative to viewer.");
                GUILayout.Label("Use scroll button during building in order to move the structure further or closer to you.");
            }

            GUILayout.Label("==============================================================");

            IgnoreTerrain = GUILayout.Toggle(IgnoreTerrain, "Allow structures to go under ground", new GUILayoutOption[0]);
            BuildAtHeight = GUILayout.Toggle(BuildAtHeight, "Enable constant build height", new GUILayoutOption[0]);
            this.Draw(modEntry);

            GUILayout.Label("==============================================================");
            GUILayout.Label("Cheats:");
            OneHitBuild = GUILayout.Toggle(OneHitBuild, "One-hit structure finish", new GUILayoutOption[0]);
            OneHitBreak = GUILayout.Toggle(OneHitBreak, "One-hit structure break", new GUILayoutOption[0]);
            //BuildingFree = GUILayout.Toggle(BuildingFree, "Crafting structures (and everything else) costs no materials", new GUILayoutOption[0]);
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

        [Draw("Set build height: ", Precision = 2, Min = -100, Max = 100)] public float BuildHeight = 2f;
        [Draw("New placement distance (0 to default): ", Precision = 1, Min = 0, Max = 9)] public float MaxPlacementDistance = 0f;
        [Draw("Target axis: ")] public Main.Axis TargetAxis = Main.Axis.X;
        [Draw("Structure edit mode: ")] public Main.RotationType StructureEditMode = Main.RotationType.Rotate;

        public bool BuildAnywhere = false;
        public bool AllowSnapping = false;
        public bool BuildAtHeight = false;
        public bool KeepLastInfo = true;
        public bool IgnoreTerrain = false;

        public bool OneHitBuild = false;
        public bool OneHitBreak = false;
        public bool BuildingFree = false;

        public KeyCode ShortcutBuildAnywhere = KeyCode.F1;
        public KeyCode ShortcutAllowSnapping = KeyCode.F4;
        public KeyCode ShortcutChangeMode = KeyCode.F2;
        public KeyCode CycleAxis = KeyCode.F3;
        public KeyCode ResetCurrentMode = KeyCode.R;
        public KeyCode ShortcutKeepLastInfo = KeyCode.K;
        public KeyCode ShortcutUnstuck = KeyCode.U;
    }
}
