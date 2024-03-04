using System;
using Beam;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using HarmonyLib;

namespace CustomDays
{
    public class Main
    {
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            Initialize(modEntry);

            Debug.Log("[SDCustomDays] Successfully started.");

            return true;
        }

        private static void Initialize(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);

            modEntryStatic = modEntry;

            settings = UnityModManager.ModSettings.Load<CustomDaySettings>(modEntry);
            timeContainer = UnityModManager.ModSettings.Load<TimeContainer>(modEntry);

            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            GameTime.TIME_SCALE = settings.timeScale;

            SaveManager.PreSave += SaveManager_PreSave;
        }

        private static void SaveManager_PreSave()
        {
            switch (Options.GeneralSettings.LastSaveSlotUsed)
            {
                case 0:
                    Main.timeContainer.timeSlotOne = Singleton<GameTime>.Instance.MilitaryTime;
                    break;

                case 1:
                    Main.timeContainer.timeSlotTwo = Singleton<GameTime>.Instance.MilitaryTime;
                    break;

                case 2:
                    Main.timeContainer.timeSlotThree = Singleton<GameTime>.Instance.MilitaryTime;
                    break;

                case 3:
                    Main.timeContainer.timeSlotFour = Singleton<GameTime>.Instance.MilitaryTime;
                    break;
            }

            timeContainer.Save(modEntryStatic);
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.OnGUI(modEntry);
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
            ApplySettings();
        }

        private static void ApplySettings()
        {
            bool isInGame = Singleton<GameTime>.Instance != null;
            float lastTimeSaved = default;

            if (isInGame) lastTimeSaved = Singleton<GameTime>.Instance.MilitaryTime;

            GameTime.TIME_SCALE = settings.timeScale;

            if (isInGame) Singleton<GameTime>.Instance.MilitaryTime = lastTimeSaved;
        }

        public static TimeContainer timeContainer;
        public static CustomDaySettings settings;
        public static UnityModManager.ModEntry modEntryStatic;

        public static Harmony harmony;
    }

    public class CustomDayHarmony
    {
        [HarmonyPatch(typeof(GameTime), nameof(GameTime.Load), new Type[] { typeof(Beam.Serialization.Json.JObject)})]
        class GameTime_Load_Patch
        {
            private static void Postfix(GameTime __instance)
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        if (Main.timeContainer.timeSlotOne != -1) __instance.MilitaryTime = Main.timeContainer.timeSlotOne;
                        break;

                    case 1:
                        if (Main.timeContainer.timeSlotTwo != -1) __instance.MilitaryTime = Main.timeContainer.timeSlotTwo;
                        break;

                    case 2:
                        if (Main.timeContainer.timeSlotThree != -1) __instance.MilitaryTime = Main.timeContainer.timeSlotThree;
                        break;

                    case 3:
                        if (Main.timeContainer.timeSlotFour != -1) __instance.MilitaryTime = Main.timeContainer.timeSlotFour;
                        break;
                }
            }
        }
    }
}