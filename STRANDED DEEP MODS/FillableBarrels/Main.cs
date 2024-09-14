using System;
using System.Reflection;
using System.IO;
using System.Linq;
using Beam;
using Beam.UI;
using UnityModManagerNet;
using UnityEngine;
using HarmonyLib;

namespace FillableBarrels
{
    public class Main
    {
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
            modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);

            Initialize(modEntry);

            return true;
        }

        private static void Initialize(UnityModManager.ModEntry modEntry)
        {
            fill1 = WavUtility.ToAudioClip(Main.ExtractEmbeddedResource("FillableBarrels.audio.fill1.wav"));
            gather1 = WavUtility.ToAudioClip(Main.ExtractEmbeddedResource("FillableBarrels.audio.gather1.wav"));
            gather2 = WavUtility.ToAudioClip(Main.ExtractEmbeddedResource("FillableBarrels.audio.gather2.wav"));

            fi_displayName = typeof(BaseObject).GetField("_displayName", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_FuelAmount = typeof(InteractiveObject_FUELCAN).GetField("_fuel", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_rain = typeof(AtmosphereStorm).GetField("_rain", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_motorfuel = typeof(MotorVehicleMovement).GetField("_fuel", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_gyrofuel = typeof(HelicopterVehicleMovement).GetField("_fuel", BindingFlags.Instance | BindingFlags.NonPublic);

            settings = UnityModManager.ModSettings.Load<FillableBarrelSettings>(modEntry);
            saveData = UnityModManager.ModSettings.Load<RainfillContainer>(modEntry);

            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            modEntryStatic = modEntry;

            SaveManager.PreSave += SaveManager_PreSave;
        }

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.OnGUI(modEntry);
        }

        public static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        public static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if (isSettingFillButton)
            {
                settings.fill = ChooseNewKey();
            }
            else if (isSettingDeductButton)
            {
                settings.deduct = ChooseNewKey();
            }

            FillableBarrelManager.HandleFillableBarrels();
        }

        private static void SaveManager_PreSave()
        {
            switch (Options.GeneralSettings.LastSaveSlotUsed)
            {
                case 0:
                    saveData.rainFillOne = FillableBarrelManager.rainTimer;
                    break;

                case 1:
                    saveData.rainFillTwo = FillableBarrelManager.rainTimer;
                    break;

                case 2:
                    saveData.rainFillThree = FillableBarrelManager.rainTimer;
                    break;

                case 3:
                    saveData.rainFillFour =  FillableBarrelManager.rainTimer;
                    break;
            }

            saveData.Save(modEntryStatic);
        }

        public static byte[] ExtractEmbeddedResource(String filename)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }
        public static void FBDoNotif(string text, float dur)
        {
            LocalizedNotification localizedNotification = new LocalizedNotification(new Notification());
            localizedNotification.Priority = NotificationPriority.Immediate;
            localizedNotification.Duration = dur;
            localizedNotification.PlayerId = PlayerRegistry.LocalPlayer.Id;
            localizedNotification.MessageText.SetTerm(text);
            localizedNotification.Raise();
        }

        public static void FBPlaySound(AudioClip audio)
        {
            AudioManager.GetAudioPlayer().Play2D(audio, AudioMixerChannels.Voice, AudioPlayMode.Single);
        }

        public static KeyCode ChooseNewKey()
        {
            var allKeys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();

            foreach (var key in allKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    isSettingFillButton = false;
                    isSettingDeductButton = false;

                    return key;
                }
            }
            return default;
        }

        public static AudioClip fill1;
        public static AudioClip gather1;
        public static AudioClip gather2;

        public static FieldInfo fi_rain;
        public static FieldInfo fi_displayName;
        public static FieldInfo fi_FuelAmount;
        public static FieldInfo fi_motorfuel;
        public static FieldInfo fi_gyrofuel;

        private static UnityModManager.ModEntry modEntryStatic;
        public static FillableBarrelSettings settings;
        public static RainfillContainer saveData;
        private static Harmony harmony;

        public static bool isSettingFillButton = false;
        public static bool isSettingDeductButton = false;
    }

    public class CustomEffectsHarmony
    {
        [HarmonyPatch(typeof(GameTime), nameof(GameTime.Load), new Type[] { typeof(Beam.Serialization.Json.JObject) })]
        class GameTime_Load_Patch
        {
            private static void Postfix(GameTime __instance)
            {
                switch (Options.GeneralSettings.LastSaveSlotUsed)
                {
                    case 0:
                        FillableBarrelManager.rainTimer = Main.saveData.rainFillOne;
                        break;

                    case 1:
                        FillableBarrelManager.rainTimer = Main.saveData.rainFillTwo;
                        break;

                    case 2:
                        FillableBarrelManager.rainTimer = Main.saveData.rainFillThree;
                        break;

                    case 3:
                        FillableBarrelManager.rainTimer = Main.saveData.rainFillFour;
                        break;
                }
            }
        }
    }
}