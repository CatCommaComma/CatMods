using UnityEngine;
using UnityModManagerNet;

namespace CustomEffects
{
    public class EffectsSettings : UnityModManager.ModSettings, IDrawable
    {
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<EffectsSettings>(this, modEntry);
		}

		public void OnChange()
		{
		}

		public void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("IMPORTANT!!! If you save and quit the game with one of the values containing a symbol that is not a number or a minus symbol, the mod will break completely!");
            GUILayout.Label("For in-depth info about the mod, please visit the mod's homepage");
            GUILayout.Label("");

            GUILayout.Label("If a setting preset is preferred, please select only one");
            EffectsManager.normalPreset = GUILayout.Toggle(EffectsManager.normalPreset, "Default settings on normal difficulty", new GUILayoutOption[0]);
            EffectsManager.hardPreset = GUILayout.Toggle(EffectsManager.hardPreset, "Default settings on hard difficulty", new GUILayoutOption[0]);
            EffectsManager.realisticPreset = GUILayout.Toggle(EffectsManager.realisticPreset, "Realistic difficulty settings (to be played with hard difficulty)", new GUILayoutOption[0]);
            GUILayout.Label("");
            sunburnEffects = GUILayout.Toggle(sunburnEffects, "Enable new sunburn effects", new GUILayoutOption[0]);
            brokenlegEffects = GUILayout.Toggle(brokenlegEffects, "Enable new broken leg effects", new GUILayoutOption[0]);
            runningDisallowed = GUILayout.Toggle(runningDisallowed, "Enable disallow running with splint", new GUILayoutOption[0]);
            GUILayout.Label("");

            if (EffectsManager.normalPreset) EffectsManager.EnableNormalPreset();

            if (EffectsManager.hardPreset) EffectsManager.EnableHardPreset();

            if (EffectsManager.realisticPreset) EffectsManager.EnableRealisticPreset();
        }

        public float UVFirst = 0f;
        public float UVSecond = 0f;
        public float UVThird = 0f;
        public float UVFourth = 0f;

        public bool firstLoad = true;

        public bool sunburnEffects = false;
        public bool brokenlegEffects = false;
        public bool runningDisallowed = false;

        [Header("")]
        [Header("Effect Settings")]
        [Draw("Thirst and hunger settings", Collapsible = true)] public ThirstAndHunger thirstAndHungerSettings = new ThirstAndHunger();

        [Draw("Healthy status settings", Collapsible = true)] public OneSetting healthySettings = new OneSetting();

        [Draw("Bleeding status settings", Collapsible = true)] public FourSettings bleedSettings = new FourSettings();
        [Draw("Poisoned status settings", Collapsible = true)] public FourSettings poisonSettings = new FourSettings();
        [Draw("Broken leg status settings", Collapsible = true)] public FourSettings brokenlegSettings = new FourSettings();
        [Draw("Broken leg status settings", Collapsible = true)] public FourSettings diarrheaSettings = new FourSettings();

        [Draw("Starvation status settings", Collapsible = true)] public OneSetting starvationSettings = new OneSetting();
        [Draw("Dehydration status settings", Collapsible = true)] public OneSetting dehydrationSettings = new OneSetting();

        [Draw("Breath boost status settings", Collapsible = true)] public FourSettings boostbreathSettings = new FourSettings();
        [Draw("Shark repellent status settings", Collapsible = true)] public FourSettings sharkrepellentSettings = new FourSettings();
        [Draw("Sunblock status settings", Collapsible = true)] public FourSettings sunblockSettings = new FourSettings();
        [Draw("Splint status settings", Collapsible = true)] public SplintSettings splintSettings = new SplintSettings();

        [Header("")]
        [Header("Effects Debugger")]
        [Draw("Debug menu", Collapsible = true)] public EffectsDebugger effectsDebugger = new EffectsDebugger();
    }

    [DrawFields(DrawFieldMask.Public)]
    public class ThirstAndHunger
    {
        [Draw("Thirst decreased per day: ", Precision = 1, Min = -3000, Max = 3000)] public float thirstPerHour = 0f;
        [Draw("Calories decreased per day: ", Precision = 1, Min = -3000, Max = 3000)] public float caloriesPerHour = 0f;
    }


    [DrawFields(DrawFieldMask.Public)]
    public class FourSettings
    {
        [Draw("Health per hour: ", Precision = 2, Min = -9999, Max = 9999)] public float healthEffect = 0f;
        [Draw("Fluids per hour: ", Precision = 2, Min = -9999, Max = 9999)] public float fluidsEffect = 0f;
        [Draw("Calories per hour: ", Precision = 2, Min = -9999, Max = 9999)] public float caloriesEffect = 0f;
        [Draw("Duration: ", Precision = 2, Min = -1, Max = 9999)] public float duration = 0f;
    }

    public class SplintSettings
    {
        [Draw("Duration: ", Precision = 2, Min = -9999, Max = 9999)] public float duration = 0f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public class OneSetting
    {
        [Draw("Health per hour: ", Precision = 2, Min = -9999, Max = 9999)] public float healthEffect = 0f;
    }

    [DrawFields(DrawFieldMask.Public)]
    public class EffectsDebugger
    {
        public enum GiveEffects
        {
            None,
            Bleeding,
            Poison,
            BrokenLeg,
            Diarrhoea,
            Splint,
            BreathBoost,
            SharkRepellent,
            Sunscreen,
            LightSunburn,
            HeavySunburn,
            RecentBrokenLeg,
            ReoccuringBrokenLeg
        };

        [Draw("Remove all effects: ")] public bool removeEffects = false;
        [Draw("Add an effect: ")] public GiveEffects givenEffect = GiveEffects.None;
    }
}