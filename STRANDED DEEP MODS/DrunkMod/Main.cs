using System;
using Beam;
using UnityModManagerNet;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;
using HarmonyLib;

namespace FoodRestoresHealth
{
    

    /*
     * 2.1.2:
     * Works fine between in-game reloads.
     * 
     * 
     * 
     * - neuzdengt teksto su tunnel visionu kai paspaudi esc
     * - test in between reloads
     */

    public class Main
    {
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);

            InitializeDrunkard(modEntry);

            return true;
        }

        private static void SaveManager_PreSave()
        {
            switch (Options.GeneralSettings.LastSaveSlotUsed)
            {
                case 0:
                    saveData.drunkLevelOne = DrunkManager._drunkLevel;
                    saveData.toleranceOne = DrunkManager._tolerance;
                    break;

                case 1:
                    saveData.drunkLevelTwo = DrunkManager._drunkLevel;
                    saveData.toleranceTwo = DrunkManager._tolerance;
                    break;

                case 2:
                    saveData.drunkLevelThree = DrunkManager._drunkLevel;
                    saveData.toleranceThree = DrunkManager._tolerance;
                    break;

                case 3:
                    saveData.drunkLevelFour = DrunkManager._drunkLevel;
                    saveData.toleranceFour = DrunkManager._tolerance;
                    break;
            }
            saveData.randomFactor = DrunkManager._randomFactor;

            saveData.Save(modEntryStatic);
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
            DrunkManager.HandleDrunkard();
        }

        private static void InitializeDrunkard(UnityModManager.ModEntry modEntry)
        {
            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            modEntryStatic = modEntry;

            SaveManager.PreSave += SaveManager_PreSave;

            settings = UnityModManager.ModSettings.Load<DrunkSettings>(modEntry);
            saveData = UnityModManager.ModSettings.Load<DrunkDataContainer>(modEntry);

            relaxedEffect = new DrunkEffects.RelaxedEffect();
            intoxicatedEffect = new DrunkEffects.IntoxicatedEffect();
            wastedEffect = new DrunkEffects.WastedEffect();
            hangoverEffect = new DrunkEffects.HangoverEffect();
            heavyhangoverEffect = new DrunkEffects.HeavyHangoverEffect();
            overdoseEffect = new DrunkEffects.AlchoholOverdoseEffect();
            overdoseRecoveryEffect = new DrunkEffects.OverdoseRecoveryEffect();

            losthallucinationEffect = new DrunkEffects.HallucinationLost();
            scaredhallucinationEffect = new DrunkEffects.HallucinationScared();
            confusedhallucinationEffect = new DrunkEffects.HallucinationConfused();
            panichallucinationEffect = new DrunkEffects.HallucinationPanic();
            terrifiedhallucinationEffect = new DrunkEffects.HallucinationTerrified();

            fi_FuelAmount = typeof(InteractiveObject_FUELCAN).GetField("_fuel", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_sleepAmount = typeof(Statistics).GetField("_sleep", BindingFlags.Instance | BindingFlags.NonPublic);
            fi_maxStamina = typeof(Statistics).GetField("_maxStamina", BindingFlags.Instance | BindingFlags.NonPublic);

            drinkAlchFem = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.drinkalchfem.wav"));
            femlaugh1 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.femlaugh1.wav"));
            femlaugh2 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.femlaugh2.wav"));
            femsigh = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.femsigh.wav"));
            fembreathe = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.fembreathe.wav"));
            femcough1 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.femcough1.wav"));
            femcough2 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.femcough2.wav"));
            femcough3 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.femcough3.wav"));

            drinkAlch = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.drinkalch.wav"));
            mansigh = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.mansigh.wav"));
            manlaugh1 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.manlaugh1.wav"));
            manlaugh2 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.manlaugh2.wav"));
            manbelch1 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.manbelch1.wav"));
            manbelch2 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.manbelch2.wav"));
            manhic1 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.manhic1.wav"));
            manbreathe = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.manbreathe.wav"));
            mancough1 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.mancough1.wav"));
            mancough2 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.mancough2.wav"));
            mancough3 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.mancough3.wav"));

            beepmult = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.beepmult.wav"));
            beepsingle = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.beepsingle.wav"));
            boarL2 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.boar2L.wav"));
            boarR2 = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.boar2R.wav"));
            splashL = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.splashL.wav"));
            splashR = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.splashR.wav"));

            halluc3sound = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.halluc3.wav"));
            halluc4sound = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.halluc4.wav"));
            halluc5sound = WavUtility.ToAudioClip(ExtractEmbeddedResource("FoodRestoresHealth.audio.halluc5.wav"));

            float num1 = 1920f / 1080f;
            float num2 = (float)Screen.width / (float)Screen.height;
            float screenRatio = num2 / num1;

            canvasParent = createCanvas(false, "drunkCanvasParent");
            canvasParent.SetActive(true);

            MakeSprite("tunnelvision", ref tunnelVision, screenRatio);
            MakeSprite("blinkdark", ref blackblink, screenRatio);
            MakeSprite("blinklight", ref whiteblink, screenRatio);

            MakeSprite("hall3", ref halluc3, screenRatio);
            MakeSprite("hall4", ref halluc4, screenRatio);
            MakeSprite("hall5", ref halluc5, screenRatio);

            UnityEngine.Object.DontDestroyOnLoad(canvasParent);
        }

        private static void MakeSprite(string name, ref Image image, float screenRatio, float alpha = 0f)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(canvasParent.transform);
            image = gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1920f * screenRatio);
            image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 1080f * screenRatio);

            Texture2D texture2D = new Texture2D(1920, 1080, TextureFormat.ARGB32, false);
            texture2D.LoadImage(ExtractEmbeddedResource("FoodRestoresHealth.assets." + name + ".png"));
            image.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 1920f, 1080f), new Vector2(860f, 540f));

            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

            image.GetComponent<CanvasRenderer>().SetAlpha(alpha);
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
        private static GameObject createCanvas(bool hide, string name = "Canvas")
        {
            GameObject gameObject = new GameObject(name);
            if (hide)
            {
                gameObject.hideFlags = HideFlags.HideAndDontSave;
            }
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.targetDisplay = 0;
            Main.addCanvasScaler(gameObject);
            Main.addGraphicsRaycaster(gameObject);
            canvas.sortingOrder = 13; //main menu is 100 or 0?..
            return gameObject;
        }

        private static void addCanvasScaler(GameObject parentCanvas)
        {
            CanvasScaler canvasScaler = parentCanvas.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.referencePixelsPerUnit = 100f;
        }

        private static void addGraphicsRaycaster(GameObject parentCanvas)
        {
            GraphicRaycaster graphicRaycaster = parentCanvas.AddComponent<GraphicRaycaster>();
            graphicRaycaster.ignoreReversedGraphics = true;
            graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
        }

        public static DrunkSettings settings;
        public static DrunkDataContainer saveData;

        public static FieldInfo fi_FuelAmount;
        public static FieldInfo fi_maxStamina;

        public static DrunkEffects.RelaxedEffect relaxedEffect;
        public static DrunkEffects.IntoxicatedEffect intoxicatedEffect;
        public static DrunkEffects.WastedEffect wastedEffect;
        public static DrunkEffects.AlchoholOverdoseEffect overdoseEffect;
        public static DrunkEffects.HangoverEffect hangoverEffect;
        public static DrunkEffects.HeavyHangoverEffect heavyhangoverEffect;
        public static DrunkEffects.OverdoseRecoveryEffect overdoseRecoveryEffect;

        public static DrunkEffects.HallucinationLost losthallucinationEffect;
        public static DrunkEffects.HallucinationScared scaredhallucinationEffect;
        public static DrunkEffects.HallucinationTerrified terrifiedhallucinationEffect;
        public static DrunkEffects.HallucinationConfused confusedhallucinationEffect;
        public static DrunkEffects.HallucinationPanic panichallucinationEffect;

        private static UnityModManager.ModEntry modEntryStatic;

        /*        Cold and wet mod       */

        public static FieldInfo fi_sleepAmount;

        /*********************************/

        public static AudioClip drinkAlchFem = null;
        public static AudioClip femsigh = null;
        public static AudioClip femlaugh1 = null;
        public static AudioClip femlaugh2 = null;
        public static AudioClip fembreathe = null;
        public static AudioClip femcough1 = null;
        public static AudioClip femcough2 = null;
        public static AudioClip femcough3 = null;

        public static AudioClip drinkAlch = null;
        public static AudioClip mansigh = null;
        public static AudioClip manlaugh1 = null;
        public static AudioClip manlaugh2 = null;
        public static AudioClip manbelch1 = null;
        public static AudioClip manbelch2 = null;
        public static AudioClip manhic1 = null;
        public static AudioClip manbreathe = null;
        public static AudioClip mancough1 = null;
        public static AudioClip mancough2 = null;
        public static AudioClip mancough3 = null;

        public static AudioClip beepmult = null;
        public static AudioClip beepsingle = null;
        public static AudioClip boarL2 = null;
        public static AudioClip boarR2 = null;
        public static AudioClip splashL = null;
        public static AudioClip splashR = null;

        public static AudioClip halluc3sound = null;
        public static AudioClip halluc4sound = null;
        public static AudioClip halluc5sound = null;

        private static GameObject canvasParent;

        internal static Image tunnelVision;
        internal static Image blackblink;
        internal static Image whiteblink;

        internal static Image halluc3;
        internal static Image halluc4;
        internal static Image halluc5;

        internal static bool debugMode = false;

    }
}

//MethodInfo mi = typeof(Statistics).GetMethod("Knockout", BindingFlags.NonPublic | BindingFlags.Instance);
//mi.Invoke(PlayerRegistry.LocalPlayer.Statistics, new object[] { });

/* 2.0.4
 * the more drunk you are, the more camera will be wobbly
 * bug fixes
 */