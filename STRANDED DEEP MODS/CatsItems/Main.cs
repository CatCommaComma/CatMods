using SDPublicFramework;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.IO;
using HarmonyLib;
using Beam;

namespace CatsItems
{
    //pocket knife looks where it should
    //New air tank works
    //skinning sharks for the final time does not break the game anymore

    public static class Main
    {
        public static CatItemsSettings Settings { get { return _settings; } }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            _settings = UnityModManager.ModSettings.Load<CatItemsSettings>(modEntry);
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(_settings.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(_settings.Save);

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

            Framework.OnFrameworkFinished += OnFrameworkDone;
            return true;
        }

        public static void OnFrameworkDone()
        {
            SetupNewAirTank();
            Framework.OnFrameworkFinished -= OnFrameworkDone;
        }

        private static void SetupNewAirTank()
        {
            try
            {
                GameObject newModel = Framework.LoadFromAssetBundles("modded_airtube");
                GameObject originalModel = Framework.GetModdedPrefab(426U);

                PrefabFactory.SetupShaders(newModel);
                MeshRenderer newRenderer = newModel.GetComponent<MeshRenderer>();
                MeshFilter newMesh = newModel.GetComponent<MeshFilter>();

                for (int i=0; i<originalModel.transform.childCount; i++)
                {
                    originalModel.transform.GetChild(i).GetComponent<MeshRenderer>().sharedMaterials = newRenderer.sharedMaterials;
                    originalModel.transform.GetChild(i).GetComponent<MeshFilter>().mesh = newMesh.mesh;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex); 
            }
        }

        private static CatItemsSettings _settings;
    }
}
