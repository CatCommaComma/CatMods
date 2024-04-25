using SDPublicFramework;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.Collections.Generic;
using System;
using System.IO;
using HarmonyLib;
using Funlabs;

namespace CatsItems
{
    /*
    
    1.0.0



    TODO
    - Proper flare effect
    - Code clean up
    - Harpoon
    - Throw aim animations
    - Whistling animation
    */

    public static class Main
    {
        public static CatItemsSettings Settings { get { return _settings; } }
        internal static Sprite SpoiledSharkBaitSprite { get { return _spoiledSharkBaitSprite; } }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            _settings = UnityModManager.ModSettings.Load<CatItemsSettings>(modEntry);
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(_settings.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(_settings.Save);

            Dictionary<uint, Type> customTypes = new Dictionary<uint, Type>()
            {
                { 405U, typeof(Cat_Bucket)},
                { 409U, typeof(Cat_Vitamins)},
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
            Stench.Create();

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
            AddSpoiledBaitIcon();
            SetupSharkFin();
            SetupMitchell();
        
            Framework.OnFrameworkFinished -= OnFrameworkDone;
        }

        private static void SetupSharkFin()
        {
            try
            {
                GameObject originalModel = Framework.GetModdedPrefab(417U);

                Texture2D cookedTexture = new Texture2D(1024, 1024);
                cookedTexture.LoadImage(CatUtility.ExtractEmbeddedResource("CatsItems.images.SharkMeatCooked_Diff.png", Assembly.GetExecutingAssembly()));

                Renderer rend = originalModel.GetComponent<Renderer>();
                Material eee = rend.sharedMaterial;
                eee.shader = Shader.Find("Beam Team/Standard/Crafting/Bumped Diffuse - Cooking");

                eee.SetTexture("_CookTex", cookedTexture);
            }

            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private static void SetupMitchell()
        {
            try
            {
                GameObject original = Framework.GetModdedPrefab(422U);


                GameObject pipi = MultiplayerMng.Instantiate<SaveablePrefab>(279U, MiniGuid.New()).gameObject;

                UnityEngine.Object.Destroy(pipi.gameObject.GetComponent<Saveable>());
                UnityEngine.Object.Destroy(pipi.gameObject.GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(pipi.gameObject.GetComponent<Collider>());

                pipi.transform.parent = original.transform;
                pipi.transform.localPosition = new Vector3(0, 0.0128f, 0.574f);
                pipi.transform.localRotation = Quaternion.identity;
                pipi.transform.Rotate(new Vector3(-90, 0, 0), Space.Self);

                pipi.SetActive(true);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
            Debug.Log("[CatsItems.SetupMitchell] If you see an error above at 'Beam.GameTime.get_Now()', it can be ignored.");
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

        private static void AddSpoiledBaitIcon()
        {
            try
            {
                Texture2D texture2D = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                texture2D.LoadImage(CatUtility.ExtractEmbeddedResource("CatsItems.images.mod_csharkbaitSPOILED.png", Assembly.GetExecutingAssembly()));
                _spoiledSharkBaitSprite = Sprite.Create(texture2D, new Rect(0f, 0f, 256f, 256f), new Vector2(0, 0));
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private static CatItemsSettings _settings;
        private static Sprite _spoiledSharkBaitSprite;
    }
}
