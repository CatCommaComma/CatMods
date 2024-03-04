using UnityEngine;
using UnityModManagerNet;
using System.IO;

namespace CustomDays
{
    public class CustomDaySettings : UnityModManager.ModSettings, IDrawable
    {
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<CustomDaySettings>(this, modEntry);
		}

		public void OnChange()
		{
		}

		public void OnGUI(UnityModManager.ModEntry modEntry)
        {
			if (calculatorTimeScale != 0)
			{
				bool displayed = false;
				float dayLength = 86400f / (float)calculatorTimeScale;

				if (dayLength > 100f) dayLength /= 60f;
				else
				{
					GUILayout.Label("Day length from calculator: " + dayLength + " seconds.");
					displayed = true;
				}

				if (dayLength > 100f) dayLength /= 60f;
				else if (!displayed)
				{
					GUILayout.Label("Day length from calculator: " + dayLength + " minutes.");
					displayed = true;
				}

				if (dayLength > 100f) dayLength /= 24f;
				else if (!displayed) GUILayout.Label("Day length from calculator: " + dayLength + " hours.");
			}
			else GUILayout.Label("Cannot divide by 0.");

			this.Draw(modEntry);
        }

		[DrawAttribute("Insert time scale to get day length in hours: ", Max = 9999, Min = 1)] public int calculatorTimeScale = 36;

		[Header("")]
		[Header("Choose Game Time Scale Here")]
		[DrawAttribute("Time scale", Max = 9999, Min = 1)] public int timeScale = 36;
	}

	public class TimeContainer : UnityModManager.ModSettings, IDrawable
	{
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<TimeContainer>(this, modEntry);
		}

		public override string GetPath(UnityModManager.ModEntry modEntry)
		{
			return Path.Combine(modEntry.Path, "TimeContainer.xml");
		}

		public void OnChange()
		{
		}

		public float timeSlotOne = -1f;
		public float timeSlotTwo = -1f;
		public float timeSlotThree = -1f;
		public float timeSlotFour = -1f;
	}
}
