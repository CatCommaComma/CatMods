using System;
using UnityEngine;
using UnityModManagerNet;

namespace FarmablePalms
{
    public class PalmSettings : UnityModManager.ModSettings, IDrawable
    {
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<PalmSettings>(this, modEntry);
		}

		public void OnChange()
		{
		}

		public bool growCoconuts = true;
		public int growCoconutsTime = 11;
		public int growFirstStages = 108;
		public int growSecondStage = 150;
		public int growThirdStage = 204;
		public int growFourthStage = 270;
	}
}
