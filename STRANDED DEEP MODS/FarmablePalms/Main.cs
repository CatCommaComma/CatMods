using System;
using UnityModManagerNet;
using UnityEngine;
using System.Reflection;
using Beam;
using HarmonyLib;
using Beam.Crafting;

namespace FarmablePalms
{
    public class Main
    {
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            settings = UnityModManager.ModSettings.Load<PalmSettings>(modEntry);
            modEntry.OnUpdate = new Action<UnityModManager.ModEntry, float>(Main.OnUpdate);
            modEntry.OnGUI = Main.OnGUI;
            modEntry.OnSaveGUI = Main.OnSaveGUI;

            harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Debug.Log("[SDFarmableTrees] Successfully started.");

            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.growCoconuts = GUILayout.Toggle(settings.growCoconuts, "Toggle regrowing coconuts", new GUILayoutOption[0]);
            if (settings.growCoconuts) UnityModManager.UI.DrawIntField(ref settings.growCoconutsTime, "Days to regrow a coconut:", null, new GUILayoutOption[0]);
            GUILayout.Label("");

            UnityModManager.UI.DrawIntField(ref settings.growFirstStages, "Hours for a palm to grow up to 1 trunk:", null, new GUILayoutOption[0]);
            UnityModManager.UI.DrawIntField(ref settings.growSecondStage, "Hours for a palm to grow up to 2 trunks:", null, new GUILayoutOption[0]);
            UnityModManager.UI.DrawIntField(ref settings.growThirdStage, "Hours for a palm to grow up to 3 trunks:", null, new GUILayoutOption[0]);
            UnityModManager.UI.DrawIntField(ref settings.growFourthStage, "Hours for a palm to grow up to 4 trunks:", null, new GUILayoutOption[0]);

            bool b = false;
            b = GUILayout.Toggle(b, "Reset settings", new GUILayoutOption[0]);
            if (b)
            {
                ResetSettings();
                b = false;
            }

            GUILayout.Label("");
            UnityModManager.UI.DrawIntField(ref days, "Calculator - insert number of days to get hours:", null, new GUILayoutOption[0]);
            GUILayout.Label("Answer is " + days * 24 + " hours.");
            GUILayout.Label("");
            //a = GUILayout.Toggle(a, "Regrow coconuts", new GUILayoutOption[0]);

            bool c = false;
            c = GUILayout.Toggle(c, "Refresh planted plants", new GUILayoutOption[0]);
            try
            {
                if (c)
                {
                    PollPalms();
                    c = false;
                }
            }
            catch (Exception ex)
            {
                c = false;
                Debug.Log("[SDFarmablePalms] Exception when trying to poll plants [OnGUI]: " + ex);
            }
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            if (!CheckIfSettingsAreRight()) ResetSettings();
            settings.Save(modEntry);
        }

        private static bool CheckIfSettingsAreRight()
        {
            if (settings.growFirstStages >= settings.growSecondStage || settings.growFirstStages >= settings.growThirdStage || settings.growFirstStages >= settings.growFourthStage)
            {
                return false;
            }
            if (settings.growSecondStage >= settings.growThirdStage || settings.growSecondStage >= settings.growFourthStage)
            {
                return false;
            }
            if (settings.growThirdStage >= settings.growFourthStage)
            {
                return false;
            }
            if (settings.growFirstStages <= 0 || settings.growSecondStage <= 0 || settings.growThirdStage <= 0 || settings.growFourthStage <= 0)
            {
                return false;
            }
            if (settings.growCoconuts)
            {
                if (settings.growCoconutsTime <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static void ResetSettings()
        {
            settings.growCoconuts = true;
            settings.growCoconutsTime = 11;
            settings.growFirstStages = 108;
            settings.growSecondStage = 150;
            settings.growThirdStage = 204;
            settings.growFourthStage = 270;
        }

        public static void PollPalms()
        {
            Plot[] plots = UnityEngine.Object.FindObjectsOfType<Plot>();
            foreach (Plot plot in plots)
            {
                Plant bb = (Plant)fi_plant.GetValue(plot);
                if (bb != null)
                {
                    PlantModel pm = (PlantModel)fi_plantModel.GetValue(bb);
                    if (pm != null)
                    {
                        pm.Poll();
                    }
                }
            }
        }

        public static void OnUpdate(UnityModManager.ModEntry modEntry, float dt)
        {
            if ((Beam.Game.State == GameState.NEW_GAME || Beam.Game.State == GameState.LOAD_GAME) && PlayerRegistry.AllPlayers.Count > 0 && Camera.main != null && Singleton<GameCalendar>.Instance != null)
            {
                if (settings.growCoconuts)
                {
                    if ((float)Singleton<GameCalendar>.Instance.DaysElapsed % (float)settings.growCoconutsTime == 0f && !oncePerDay && Singleton<GameCalendar>.Instance.DaysElapsed != 0)
                    {
                        InteractiveObject_PALM[] palms = UnityEngine.Object.FindObjectsOfType<InteractiveObject_PALM>();
                        foreach (InteractiveObject_PALM palm in palms)
                        {
                            int a = r.Next(0, 2);
                            if (a == 0)
                            {
                                MethodInfo mi = typeof(InteractiveObject_PALM).GetMethod("GenerateFruit", BindingFlags.NonPublic | BindingFlags.Instance);
                                mi.Invoke(palm, new object[] { });
                            }
                        }
                        oncePerDay = true;
                    }

                    if ((float)Singleton<GameCalendar>.Instance.DaysElapsed % (float)settings.growCoconutsTime == 1f && oncePerDay && Singleton<GameCalendar>.Instance.DaysElapsed != 1) oncePerDay = false;
                }
            }
        }

        public static Farming_PLOT CatMagic(GameObject[] plantStages)
        {
            Plant aa = plantStages[4].transform.parent.gameObject.GetComponent<Plant>();
            Farming_PLOT d = null;
            bool flag11 = aa == null;
            if (!flag11)
            {
                Plot[] plots = UnityEngine.Object.FindObjectsOfType<Plot>();
                foreach (Plot plot in plots)
                {
                    Plant bb = (Plant)fi_plant.GetValue(plot);
                    if (bb == aa)
                    {
                        Farming_PLOT[] fplots = UnityEngine.Object.FindObjectsOfType<Farming_PLOT>();
                        foreach (Farming_PLOT fplot in fplots)
                        {
                            Plot cc = (Plot)fi_plot.GetValue(fplot);
                            if (cc == plot)
                            {
                                d = fplot;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return d;
        }

        public static PalmSettings settings;
        private static Harmony harmony;
        private static System.Random r = new System.Random();

        public static FieldInfo fi_plot = typeof(Farming_PLOT).GetField("_plot", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo fi_plant = typeof(Plot).GetField("_plant", BindingFlags.Instance | BindingFlags.NonPublic);
        public static FieldInfo fi_plantModel = typeof(Plant).GetField("_plantModel", BindingFlags.Instance | BindingFlags.NonPublic);

        private static int days = 0;
        private static bool oncePerDay = false;
    }

    public class FarmablePalm : MonoBehaviour
    {
        public GameObject _farmablePalm = null;
        public GameObject _farmablePalm2 = null;
        public GameObject _farmablePalm3 = null;
        public GameObject _farmablePalm4 = null;

        public GameObject _farmablePalmReal = null;
        public GameObject _farmablePalmRealSize2 = null; //2 trunks high
        public GameObject _farmablePalmRealSize3 = null; //3 trunks high
        public GameObject _farmablePalmRealSize4 = null; //4 trunks high
    }

    public class PlotPalm : MonoBehaviour
    {
        public Farming_PLOT catLopPlot;
        public PlantModel catPalmPlantModel;
    }
}
