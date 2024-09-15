using UnityEngine;
using UnityModManagerNet;
using System.IO;
using Beam;

namespace FoodRestoresHealth
{
    public class DrunkSettings : UnityModManager.ModSettings, IDrawable
    {
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<DrunkSettings>(this, modEntry);
		}

		public void OnChange()
		{
		}

		public void OnGUI(UnityModManager.ModEntry modEntry)
        {
			if (Main.debugMode)
            {
				bool addDrunk = false;
				bool resetDrunk = false;
				bool fillAllCans = false;
				bool triggerevent = false;
				bool randomhallucination = false;

				addDrunk = GUILayout.Toggle(addDrunk, "Add Drunk Level", new GUILayoutOption[0]);
				resetDrunk = GUILayout.Toggle(resetDrunk, "Reset Drunk Levels", new GUILayoutOption[0]);
				fillAllCans = GUILayout.Toggle(fillAllCans, "Fill all gas cans", new GUILayoutOption[0]);
				triggerevent = GUILayout.Toggle(triggerevent, "Trigger an event", new GUILayoutOption[0]);
				randomhallucination = GUILayout.Toggle(randomhallucination, "Random hallucination", new GUILayoutOption[0]);

				if (addDrunk) DrunkManager.UpdateDrunkStats(PlayerRegistry.LocalPlayer, 1);

				if (resetDrunk) DrunkManager.ResetDrunk(PlayerRegistry.LocalPlayer.Statistics, false);

				if (fillAllCans)
                {
					InteractiveObject_FUELCAN[] cans = UnityEngine.Object.FindObjectsOfType<InteractiveObject_FUELCAN>();

					foreach (InteractiveObject_FUELCAN can in cans)
					{
						Main.fi_FuelAmount.SetValue(can, 4f);
					}
				}

				if (triggerevent) DrunkManager.DoEvent();

				if (randomhallucination) DrunkManager.PlayRandomHallucination();
            }

			usingWetMod = GUILayout.Toggle(usingWetMod, "Using Hantacore's wet and cold mod?", new GUILayoutOption[0]);

			this.Draw(modEntry);
        }

		public bool usingWetMod = false;
	}

	public class DrunkDataContainer : UnityModManager.ModSettings, IDrawable
    {
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<DrunkDataContainer>(this, modEntry);
		}

		public override string GetPath(UnityModManager.ModEntry modEntry)
		{
			return Path.Combine(modEntry.Path, "DrunkData.xml");
		}

		public void OnChange()
		{
		}

		public int randomFactor = 0;

		public int toleranceOne = 0;
		public int drunkLevelOne = 0;

		public int toleranceTwo = 0;
		public int drunkLevelTwo = 0;

		public int toleranceThree = 0;
		public int drunkLevelThree = 0;

		public int toleranceFour = 0;
		public int drunkLevelFour = 0;
	}
}
