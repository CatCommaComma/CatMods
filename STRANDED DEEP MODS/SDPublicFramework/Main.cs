using System;
using Beam;
using System.Collections.Generic;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using HarmonyLib;
using Beam.Utilities;
using Beam.Utilities.Reflection;

//1.0
//show equipped items in inventory UI -
//fishies
//modded items get highlighted
//lod control
//generate items on land by given parameters -

//4.0.3:
//initial load might take more but the rest of the game is significantly faster
//better functionality for using vanilla items for modded framework
//i don't remember what i changed
//log all information of raycasted gameobject in player.log
//you can spawn stuff from vanilla developer console

//TO DO ATLEAST: vanilla, construction
namespace SDPublicFramework
{
    public class Main
    {
        public static bool StrandedWideOn { get { return IsStrandedWide(); } }

        internal static Dictionary<Enum, DescriptionAttribute> CustomDescriptionAttributes { get { return _customDescriptionAttributes; } }
        internal static CatSettings ModSettings { get { return _settings; } }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            _settings = UnityModManager.ModSettings.Load<CatSettings>(modEntry);
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(_settings.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(_settings.Save);
            modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);

            Harmony harmony = new Harmony(modEntry.Info.Id);

            harmony.PatchAll(Assembly.GetExecutingAssembly());
            MethodInfo og = typeof(Reflection).GetMethod("GetAttribute", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(typeof(DescriptionAttribute));
            MethodInfo prefix = typeof(Main).GetMethod("GetAttributePrefix", BindingFlags.NonPublic | BindingFlags.Static);

            harmony.Patch(og, prefix: new HarmonyMethod(prefix));

            Framework.StartupFramework();
            BasicSounds.Initialize();

            return true;
        }

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            HandleEquippables();
        }

        private static void HandleEquippables()
        {
            if ((Game.State == GameState.LOAD_GAME || Game.State == GameState.NEW_GAME))
            {
                if (Input.GetKeyDown(_settings.UnequipApparelKey)) EquippableSystem.UnequipAll();
            }
        }

        private static bool GetAttributePrefix(Enum enumerator, ref DescriptionAttribute __result)
        {
            if (enumerator.GetType().GetField(enumerator.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault_NonAlloc() is DescriptionAttribute obj)
            {
                __result = obj;
                return false;
            }
            else if (_customDescriptionAttributes.TryGetValue(enumerator, out DescriptionAttribute descriptionAttribute))
            {
                __result = descriptionAttribute;
                return false;
            }
            __result = default(DescriptionAttribute);
            return false;
        }

        private static bool IsStrandedWide()
        {
            UnityModManager.ModEntry wide = UnityModManager.FindMod("StrandedWideMod");
            return (wide != null && wide.Active && wide.Loaded);
        }

        private static Dictionary<Enum, DescriptionAttribute> _customDescriptionAttributes = new Dictionary<Enum, DescriptionAttribute> { };
        private static CatSettings _settings;
    }
}