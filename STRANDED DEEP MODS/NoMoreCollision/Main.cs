using UnityModManagerNet;
using System.Reflection;
using HarmonyLib;
using System;
using UnityEngine;
using Beam;
using System.Threading.Tasks;

namespace NoMoreCollision
{
    //more performant code
    //tidy up code
    //publish to git
    //wall light hook can't be rotated
    //structure imposters have the old scale image
    //stairs don't keep last info scale/rotation
    //trying to snap to structures, even foundations, built with build anywhere mode with non-build-anywhere structures will keep the player in building mode (can't do anything other than walk and enter main menu). Please save before trying to mix structure types and use the unstuck button.

    public class Main
    {
        public static bool BuildAnywhere { get { return _settings.BuildAnywhere; } }
        public static bool AllowSnapping { get { return _settings.AllowSnapping; } }
        public static bool KeepLastInfo { get { return _settings.KeepLastInfo; } }
        public static bool IgnoreTerrain { get { return _settings.IgnoreTerrain; } }
        public static Axis TargetAxis { get { return _settings.TargetAxis; } }
        public static RotationType StructureEditMode { get { return _settings.StructureEditMode; } }

        public static bool BuildAtHeight { get { return _settings.BuildAtHeight; } }
        public static float BuildHeight { get { return _settings.BuildHeight; } }

        public static bool OneHitBuild { get { return _settings.OneHitBuild; } }
        public static bool OneHitBreak { get { return _settings.OneHitBreak; } }
        public static bool BuildingFree { get { return _settings.BuildingFree; } }

        public static float PlacingDistance { get { return _settings.MaxPlacementDistance; } }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            _settings = UnityModManager.ModSettings.Load<NoCollisionSettings>(modEntry);

            modEntry.OnGUI = _settings.OnGUI;
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(_settings.Save);
            modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);

            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(_settings.ShortcutBuildAnywhere))
            {
                if (Crafter_PlaceCraftable_Sequence_Patch.IsPlacing)
                {
                    DoSpeech($"Cannot change build anywhere mode during building phase.");
                    return;
                }

                _settings.BuildAnywhere = !_settings.BuildAnywhere;
                DoSpeech($"{(_settings.BuildAnywhere ? "ENABLED" : "DISABLED")} build anywhere.");
            }

            if (Input.GetKeyDown(_settings.ShortcutAllowSnapping))
            {
                _settings.AllowSnapping = !_settings.AllowSnapping;
                DoSpeech($"{(_settings.AllowSnapping ? "ENABLED" : "DISABLED")} snapping.{(_settings.BuildAnywhere? "": " [ONLY BUILD ANYWHERE MODE]")}");
            }

            if (Input.GetKeyDown(_settings.CycleAxis))
            {
                switch (_settings.TargetAxis)
                {
                    case Axis.X:
                        _settings.TargetAxis = Axis.Y;
                        break;
                    case Axis.Y:
                        _settings.TargetAxis = Axis.Z;
                        break;
                    case Axis.Z:
                        _settings.TargetAxis = Axis.X;
                        break;
                }

                DoSpeech($"Target: {(_settings.TargetAxis)}");
            }

            if (Input.GetKeyDown(_settings.ShortcutChangeMode))
            {
                if (_settings.StructureEditMode == RotationType.Rotate) _settings.StructureEditMode = RotationType.Scale;
                else                                                    _settings.StructureEditMode = RotationType.Rotate;

                DoSpeech($"{(_settings.StructureEditMode)} mode.{(_settings.BuildAnywhere ? "" : " [ONLY BUILD ANYWHERE MODE]")}");
            }

            if (Input.GetKeyDown(_settings.ResetCurrentMode))
            {
                Constructing_OwnerPlacing_Patch.Reset(_settings.StructureEditMode, _settings.TargetAxis);
            }

            if (Input.GetKeyDown(_settings.ShortcutKeepLastInfo))
            {
                _settings.KeepLastInfo = !_settings.KeepLastInfo;
                DoSpeech($"{(_settings.KeepLastInfo ? "ENABLED" : "DISABLED")} keep last info.");
            }

            if (Crafter_PlaceCraftable_Sequence_Patch.IsPlacing && Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                _settings.MaxPlacementDistance += Input.GetAxis("Mouse ScrollWheel") * dt * 120f;
                _settings.MaxPlacementDistance = Mathf.Clamp(_settings.MaxPlacementDistance, 0, 20);
            }

            if (Input.GetKeyDown(_settings.ShortcutUnstuck))
            {
                try
                {
                    Crafter_PlaceCraftable_Sequence_Patch.Unstuck();
                    DoSpeech($"[UNSTUCK] Success.");
                }
                catch (Exception ex)
                {
                    Debug.Log("[NoMoreCollision.UNSTUCK] UNSTUCK DID NOT WORK!!! " + ex);
                    DoSpeech($"[UNSTUCK] Error detected. Send player.log file to creator.");
                }
            }
        }

        public static void DoSpeech(string text = "Unspecified text", int t = 1300)
        {
            if (PlayerRegistry.LocalPlayer != null)
            {
                PlayerSpeech ps = PlayerRegistry.LocalPlayer.PlayerSpeech;
                ps.View.SubtitlesLabel.Text = text;
                ps.View.Show();
                Task.Delay(t).ContinueWith(delegate (Task task) { if (text == ps.View.SubtitlesLabel.Text) ps.View.Hide(); });
            }
        }

        public enum RotationType
        {
            Rotate,
            Scale
        }

        public enum Axis
        {
            X,
            Y,
            Z
        }

        private static NoCollisionSettings _settings;
        internal static bool ChangingResetKey = false;
        internal static bool ChangingBuildAnywhereKey = false;
        internal static bool ChangingSnappingKey = false;
        internal static bool ChangingModeKey = false;
        internal static bool ChangingCicleAxisKey = false;
        internal static bool ChangingKeepLastInfoKey = false;
        internal static bool ChangingUnstuckKey = false;
    }
}
