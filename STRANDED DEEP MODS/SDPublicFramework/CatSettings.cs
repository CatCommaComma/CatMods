using Beam;
using UnityEngine;
using UnityModManagerNet;

namespace SDPublicFramework
{
    public class CatSettings : UnityModManager.ModSettings, IDrawable
    {
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save<CatSettings>(this, modEntry);
        }

        public void OnChange()
        {
        }

        public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            Main.ChangingUnequipKey = GUILayout.Toggle(Main.ChangingUnequipKey, "Change the unequip key", new GUILayoutOption[0]);
            GUILayout.Label("Current unequip key: " + UnequipApparelKey);
            GUILayout.Label("");

            PrefabDebugger = GUILayout.Toggle(PrefabDebugger, " SDPF Prefab debugger (for modders)", new GUILayoutOption[0]);

            if (PrefabDebugger)
            {
                GUILayout.Label("SDPF TOOLS");
                GUILayout.Label("");
                GetCurrentPrefab = GUILayout.Toggle(GetCurrentPrefab, "Get currently held prefab as chosen prefab", new GUILayoutOption[0]);
                GetCurrentRaycastedPrefab = GUILayout.Toggle(GetCurrentRaycastedPrefab, "Get currently raycasted prefab as base prefab", new GUILayoutOption[0]);
                GetChosenPrefabInfo = GUILayout.Toggle(GetChosenPrefabInfo, "Get chosen prefab's info", new GUILayoutOption[0]);
                GUILayout.Label("");

                Main.ModSettings.Draw(modEntry);
                GUILayout.Label("");

                GUILayout.Label("INSTANTIATE PREFABS");
                GUILayout.Label("");
                InstantiatePrefab = GUILayout.Toggle(InstantiatePrefab, " Instantiate prefab", new GUILayoutOption[0]);
                GUILayout.Label("Instantiation prefab:");
                CatUtility.InstantiationPrefab = GUILayout.TextField(CatUtility.InstantiationPrefab, 70, new GUILayoutOption[0]);
                GUILayout.Label("");

                GUILayout.Label("CACHED PREFAB INFORMATION");
                GUILayout.Label("Chosen prefab name: " + CatUtility.ChosenPrefabName);
                GUILayout.Label("Chosen prefab prefab path: " + CatUtility.ChosenPrefabPrefabPath);
                GUILayout.Label("Chosen prefab icon path: " + CatUtility.ChosenPrefabIconPath);
                GUILayout.Label($"Chosen prefab crafting type: {CatUtility.ChosenPrefabCraftingType[0]} | {CatUtility.ChosenPrefabCraftingType[1]} [{(AttributeType)CatUtility.ChosenPrefabCraftingType[0]} | {(InteractiveType)CatUtility.ChosenPrefabCraftingType[1]}]");
                GUILayout.Label("");
                GUILayout.Label("Chosen base prefab name: " + CatUtility.ChosenBasePrefabName);
                GUILayout.Label("Chosen base prefab id: " + CatUtility.ChosenBasePrefabId);
                GUILayout.Label("Chosen base prefab reference: " + CatUtility.ChosenBasePrefabMiniGuid.ToString());
            }

            CatUtility.HandlePrefabDebugger(this, PrefabAdjuster);
        }

        public bool UsingSwide = false;
        public KeyCode UnequipApparelKey = KeyCode.Z;

        public bool PrefabDebugger;

        public bool GetCurrentPrefab;
        public bool GetChosenPrefabInfo;
        public bool GetCurrentRaycastedPrefab;

        [Draw("Adjust prefabs", Collapsible = true)] public PrefabAdjuster PrefabAdjuster = new PrefabAdjuster();

        public bool InstantiatePrefab;
    }

    [DrawFields(DrawFieldMask.Public)]
    public class PrefabAdjuster
    {
        [Draw("Update chosen prefab")] public bool UpdatePrefabInfo = false;
        [Draw("Reset chosen prefab info")] public bool ResetPrefabInfo = false;

        [Draw("Hold position x:", Precision = 4, Min = -360, Max = 360)] public float PositionX = 0f;
        [Draw("Hold position y:", Precision = 4, Min = -360, Max = 360)] public float PositionY = 0f;
        [Draw("Hold position z:", Precision = 4, Min = -360, Max = 360)] public float PositionZ = 0f;

        [Draw("Hold rotation x:", Precision = 4, Min = -360, Max = 360)] public float RotationX = 0f;
        [Draw("Hold rotation y:", Precision = 4, Min = -360, Max = 360)] public float RotationY = 0f;
        [Draw("Hold rotation z:", Precision = 4, Min = -360, Max = 360)] public float RotationZ = 0f;

        [Draw("Idle animation:", Min = 0)] public int IdleAnimation = 0;
        [Draw("Swing at nothing animation:", Min = 0)] public int EmptySwingAnimation = 0;
        [Draw("Swing at something animation:", Min = 0)] public int HitSwingAnimation = 0;
    }
}