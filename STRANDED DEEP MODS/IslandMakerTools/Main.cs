using System.Reflection;
using UnityModManagerNet;
using HarmonyLib;
using System;
using UnityEngine;
using Beam;

namespace IslandMakerTools
{
    //allows some things to be moved/rotated on all axises
    //allows object scaling directly in the island editor (spherical object colliders will not be accurate, this is how unity engine works)
    //allows infinite amount of things placed
    //expanded camera movement bounds

    //1.0.1
    //fixed scaling button resetting back to unavailable after reloading island maker
    //added scale on all sides option
    //added configurable quick-select scaling mode button

    //note: cannot make terrain be raisable to bigger extremes for non-modded players due to how interpretation of height works
    public class Main
    {
        public static bool ScaleOnAllSides { get { return _settings.ScaleOnAllSides; } }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            _settings = UnityModManager.ModSettings.Load<IMTSettings>(modEntry);
            modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);
            modEntry.OnGUI = _settings.OnGUI;

            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            return true;
        }

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (Input.GetKeyDown(_settings.ShortcutScale) && Game.State == GameState.MAP_EDITOR)
            {
                LE_GUIInterface_Awake_Patch.QuickScale();
            }
        }

        private static IMTSettings _settings;
    }
}
