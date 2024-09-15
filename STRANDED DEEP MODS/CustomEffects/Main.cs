using Beam;
using System;
using UnityModManagerNet;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;

namespace CustomEffects
{
    internal class Main
    {
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            effectsSettings = UnityModManager.ModSettings.Load<EffectsSettings>(modEntry);

            modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);

            Initialize(modEntry);

            Debug.Log("[SDCustomEffects] Successfully started.");

            return true;
        }

        public static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            EffectsManager.UpdateNewEffects();
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            effectsSettings.Save(modEntry);
            EffectsManager.ApplySettings();
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            effectsSettings.OnGUI(modEntry);
            effectsSettings.Draw(modEntry);

            EffectsManager.EffectDebugger();
        }

        private static void Initialize(UnityModManager.ModEntry modEntry)
        {
            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (statusEffects == null) EffectsManager.InitializeEffectInformation();

            SaveManager.PreSave += SaveManager_PreSave;

            EffectsManager.fi_sprintButtonDown = typeof(Movement).GetField("_sprintButtonDown", BindingFlags.NonPublic | BindingFlags.Instance);

            EffectsManager.fi_caloriePerDay = typeof(Statistics).GetField("CALORIE_POINTS_PER_DAY", BindingFlags.NonPublic | BindingFlags.Instance);
            EffectsManager.fi_thirstPerDay = typeof(Statistics).GetField("THIRST_POINTS_PER_DAY", BindingFlags.NonPublic | BindingFlags.Instance);

            sunburnLightEffect = new EffectsHolder.SunburnLight();
            sunburnHeavyEffect = new EffectsHolder.SunburnHeavy();
            recentBrokenLegEffect = new EffectsHolder.RecentBrokenLeg();
            recentBrokenLegExtraEffect = new EffectsHolder.RecentBrokenLegExtra();

            if (effectsSettings.firstLoad)
            {
                EffectsManager.EnableNormalPreset();
                effectsSettings.firstLoad = false;

                effectsSettings.Save(modEntry);
            }

            EffectsManager.ApplySettings();
        }

        private static void SaveManager_PreSave()
        {
            if (Options.GeneralSettings.LastSaveSlotUsed == 0) effectsSettings.UVFirst = EffectsManager.playerUV;
            else if (Options.GeneralSettings.LastSaveSlotUsed == 1) effectsSettings.UVSecond = EffectsManager.playerUV;
            else if (Options.GeneralSettings.LastSaveSlotUsed == 2) effectsSettings.UVThird = EffectsManager.playerUV;
            else if (Options.GeneralSettings.LastSaveSlotUsed == 3) effectsSettings.UVFourth = EffectsManager.playerUV;
        }

        public static EffectsHolder.SunburnLight sunburnLightEffect;
        public static EffectsHolder.SunburnHeavy sunburnHeavyEffect;
        public static EffectsHolder.RecentBrokenLeg recentBrokenLegEffect;
        public static EffectsHolder.RecentBrokenLegExtra recentBrokenLegExtraEffect;

        public static EffectsSettings effectsSettings;

        private static Harmony harmony;

        public static List<EffectInformation> statusEffects;
    }
}