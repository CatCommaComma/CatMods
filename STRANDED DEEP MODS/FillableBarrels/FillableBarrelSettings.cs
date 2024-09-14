using System.IO;
using UnityModManagerNet;
using UnityEngine;

namespace FillableBarrels
{
	public class FillableBarrelSettings : UnityModManager.ModSettings, IDrawable
	{
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<FillableBarrelSettings>(this, modEntry);
		}

		public void OnGUI(UnityModManager.ModEntry modEntry)
		{
			GUILayout.Label("The maximum water amount in a barrel is 60 servings.");
			GUILayout.Label("The maximum fuel amount in a barrel is 40 liters.");
			GUILayout.Label("");
			GUILayout.Label("This mod is very dependant on barrel names, DO NOT rename the barrel unless you intend to lose its held contents!!!");
			GUILayout.Label("If you want to convert a barrel to another liquid type, rename it or fill it with something when its total liquid value is 0.");
			GUILayout.Label("");

			Main.isSettingFillButton = GUILayout.Toggle(Main.isSettingFillButton, "Set new fill barrel button.", new GUILayoutOption[0]);
			Main.isSettingDeductButton = GUILayout.Toggle(Main.isSettingDeductButton, "Set new deduct from barrel/vehicle button.", new GUILayoutOption[0]);

			if (Main.isSettingFillButton || Main.isSettingDeductButton)
            {
				GUILayout.Label("Enter a key and it will be saved as a keybind...");
            }
			else
            {
				GUILayout.Label("Current fill button: " + fill);
				GUILayout.Label("Current deduct button: " + deduct);
			}

			this.Draw(modEntry);
        }

		public void OnChange()
		{
		}

		[Header("")]
		[DrawAttribute("How many in-game hours should pass before water containers get refilled: ", Precision = 2, Max = 20, Min = 0.01)] public float refillRate = 1.5f;

		public KeyCode fill = KeyCode.Mouse2;
		public KeyCode deduct = KeyCode.Mouse1;	
	}

	public class RainfillContainer : UnityModManager.ModSettings, IDrawable
	{
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<RainfillContainer>(this, modEntry);
		}

		public override string GetPath(UnityModManager.ModEntry modEntry)
		{
			return Path.Combine(modEntry.Path, "RefillTimers.xml");
		}

		public void OnChange()
		{
		}

		public float rainFillOne = 0f;
		public float rainFillTwo = 0f;
		public float rainFillThree = 0f;
		public float rainFillFour = 0f;
	}
}
