using SDPublicFramework;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.IO;
using HarmonyLib;

namespace CatsItems
{
    public static class Main
    {
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            Dictionary<uint, Type> customTypes = new Dictionary<uint, Type>()
            {
                { 405U, typeof(Cat_Bucket)},
                { 413U, typeof(Cat_Lighter)},
                { 414U, typeof(Cat_SharkSign)},
                { 415U, typeof(Cat_Flippers)},
                { 416U, typeof(Cat_Goggles)},
                { 418U, typeof(Cat_Whistle)},
                { 419U, typeof(Cat_Flare)},
                { 420U, typeof(Cat_GoggleLight)},
                { 421U, typeof(Cat_SharkBait)},
                { 422U, typeof(Cat_StarRemover)},
                { 427U, typeof(Cat_Harpoon)},
            };

            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            LocalCraftingLogic.Initialize();
            LocalEmbeddedAudio.Initialize();
            Vitamins.Create();

            Framework.RegisterForInjection(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), customTypes);
            Framework.IncludeIConsumableSound(404U, LocalEmbeddedAudio.DrinkWaterBottle);
            Framework.IncludeIConsumableSound(406U, LocalEmbeddedAudio.PopPillsSound);
            Framework.IncludeIConsumableSound(407U, LocalEmbeddedAudio.MorphineInjectionSound);
            Framework.IncludeIConsumableSound(409U, LocalEmbeddedAudio.PopPillsSound);

            return true;
        }

        public static KeyCode TurnOnGoggleLightKey = KeyCode.X;
    }
}
