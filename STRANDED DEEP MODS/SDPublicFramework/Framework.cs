using System;
using Beam;
using UnityEngine;
using Beam.Crafting;
using System.IO;
using System.Collections.Generic;
using Beam.UI;
using System.Reflection;

namespace SDPublicFramework
{
    public static partial class Framework
    {
        public static bool FrameworkInitialized { get { return _frameworkInitialized; } }

        public static Dictionary<string, Func<BaseObject, CraftingCombination, Crafter, IPlayer, IList<IBase>, bool>> CraftingLogic { get { return _craftingLogic; } }
        internal static Dictionary<uint, float> ModdedItemLootTable { get { return _moddedContainerItems; } }
        internal static List<LootSpawn.LootItem> AdditionalVanillaItemLootTable { get { return _containerItems; } }
        internal static List<Sprite> SubcategoryIcons { get { return _subcategoryIcons; } }
        internal static List<Sprite> SmallIcons { get { return _smallIcons; } }
        internal static List<Sprite> MediumIcons { get { return _mediumIcons; } }

        internal static List<AssetBundle> AssetBundles { get { return _assetBundles; } }
        internal static Dictionary<uint, Type> CustomTypes { get { return _customTypes; } }

        /// <summary> This is intended for non-custom-type consumables. Can be added any time.</summary>
        public static void IncludeIConsumableSound(uint itemId, AudioClip sound)
        {
            if (_customClips.ContainsKey(itemId)) Logger.Warning($"Item of id '{itemId}' already has a sound linked to it.");
            _customClips.Add(itemId, new CustomSound(sound));
        }

        public static GameObject GetModdedPrefab(string name)
        {
            if (_nameToId.ContainsKey(name)) return _moddedPrefabs[_nameToId[name]];
            else Logger.Warning($"Item of name '{name}' is not in the prefab list.");
            return null;
        }

        public static GameObject GetModdedPrefab(uint id)
        {
            if (_moddedPrefabs.ContainsKey(id)) return _moddedPrefabs[id];
            else Logger.Warning($"Item of id '{id}' is not in the prefab list.");
            return null;
        }

        internal static void StartupFramework()
        {
            _frameworkInitialized = true;
            PrefabFactory.InitializeFieldInfos();

            Logger.Log("SDPF startup done.");
        }

        /// <summary> Path can be passed with 'Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)'. It is the root mod folder from which other assets and folders branch out. You can provide another path if you know what you're doing.</summary>
        public static void RegisterForInjection(string path, Dictionary<uint, Type> customTypes = null)
        {
            if (!_frameworkInitialized)
            {
                Logger.Exception("Framework isn't ready for registration.");
                return;
            }
            _registeredPaths.Add(path);

            if (customTypes != null)
            {
                foreach (KeyValuePair<uint, Type> type in customTypes)
                {
                    if (!_customTypes.ContainsKey(type.Key))
                    {
                        _customTypes.Add(type.Key, type.Value);
                    }
                    else
                    {
                        Logger.Exception($"There already is an object that has a custom type [{_customTypes[type.Key].Name}] assigned to this id [{type.Key}].");
                    }
                }
            }
        }

        internal static void InjectModsToFramework()
        {
            Logger.Log($"Found {_registeredPaths.Count} registered mods in total.");
            ProcessDrops();

            foreach (string path in _registeredPaths)
            {
                Logger.Log($"Trying to inject mod of folder '{Path.GetFileNameWithoutExtension(path)}':");

                try
                {
                    string assetsPath = Path.Combine(path, "SDPFassets");

                    ProcessBundle(assetsPath);
                    ProcessIcons(assetsPath);
                    ProcessCombinations(assetsPath);
                    ProcessPrefabInfo(assetsPath);

                    Logger.Log($"Injection passed.");
                }
                catch (Exception ex)
                {
                    Logger.Exception($"Failed to load mod because of error while injecting: " + ex);
                }
            }

            //foreach (AssetBundle bundle in _assetBundles) bundle.Unload(false);

            PrefabFactory.CleanupReflection();
            _registeredPaths.Clear();
            _customTypes.Clear();
            //_assetBundles.Clear();

            _registeredPaths = null;
            _customTypes = null;
            //_assetBundles = null;
            
            Logger.Log("Passed through injection of all registered mods.");
        }

        private static void ProcessBundle(string assetsPath)
        {
            string bundlePath = Path.Combine(assetsPath, "bundles");

            if (Directory.Exists(bundlePath))
            {
                string[] bundleNames = Directory.GetFiles(bundlePath);
                Logger.Log($"Bundles: Detected {bundleNames.Length} files in given directory.");

                foreach (string bundleName in bundleNames)
                {
                    byte[] memory = CatUtility.ExtractNonEmbeddedResource(bundleName);
                    if (memory != default)
                    {
                        Logger.Log($"Bundles: Initializing '{Path.GetFileNameWithoutExtension(bundleName)}' asset bundle.");

                        try
                        {
                            _assetBundles.Add(AssetBundle.LoadFromMemory(memory));

                            string[] allModdedItems = _assetBundles[_bundleCount].GetAllAssetNames();
                            Logger.Log($"Bundles: Found {allModdedItems.Length} assets.");

                        }
                        catch (Exception ex)
                        {
                            Logger.Exception($"Bundles: Error processing this ^ asset bundle: \n {ex}");
                        }
                    }
                    _bundleCount++;
                }
                Logger.Log("Bundles: OK");
            }
            else
            {
                Logger.Warning("Bundles: Directory not found - no bundles will be loaded.");
            }
        }

        private static void ProcessIcons(string assetsPath)
        {
            string subcategoryPath = System.IO.Path.Combine(assetsPath, "icons\\subcategories");
            string smallPath = System.IO.Path.Combine(assetsPath, "icons\\small");
            string mediumPath = System.IO.Path.Combine(assetsPath, "icons\\medium");

            if (Directory.Exists(subcategoryPath) || (Directory.Exists(smallPath) && Directory.Exists(mediumPath)))
            {
                string[] filePaths = Directory.GetFiles(subcategoryPath);
                foreach (string filePath in filePaths)
                {
                    Sprite newSprite = default;
                    AddIcon(ModdedIconType.SUBCATEGORY, filePath, newSprite);
                }

                filePaths = Directory.GetFiles(smallPath);
                foreach (string filePath in filePaths)
                {
                    Sprite newSprite = default;
                    AddIcon(ModdedIconType.SMALL, filePath, newSprite);
                }

                filePaths = Directory.GetFiles(mediumPath);
                foreach (string filePath in filePaths)
                {
                    Sprite newSprite = default;
                    AddIcon(ModdedIconType.MEDIUM, filePath, newSprite);
                }
                Logger.Log("Icons: OK");
            }
            else
            {
                Logger.Warning($"Icons: Icon folders missing (false if doesn't exist):\nsubcategory - {Directory.Exists(subcategoryPath)}, small - {Directory.Exists(smallPath)}, medium - {Directory.Exists(mediumPath)}. Ignore if intentional.");
            }
        }

        private static void AddIcon(ModdedIconType type, string imagePath, Sprite sprite)
        {
            try
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(imagePath);
                Texture2D texture2D = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                texture2D.LoadImage(CatUtility.ExtractNonEmbeddedResource(imagePath));

                if (type == ModdedIconType.SUBCATEGORY)
                {
                    sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 128f, 128f), new Vector2(860f, 540f));
                    sprite.name = name;

                    _subcategoryIcons.Add(sprite);
                }
                else if (type == ModdedIconType.SMALL)
                {
                    sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 128f, 128f), new Vector2(860f, 540f));
                    sprite.name = name;

                    _smallIcons.Add(sprite);
                }
                else if (type == ModdedIconType.MEDIUM)
                {
                    sprite = Sprite.Create(texture2D, new Rect(0f, 0f, 256f, 256f), new Vector2(860f, 540f));
                    sprite.name = name;

                    _mediumIcons.Add(sprite);
                }
            }
            catch (Exception ex)
            {
                Logger.Exception($"Icons: Exception trying to process icon. Icon path: {imagePath}, Exception: \n {ex}");
            }
        }

        private static void ProcessCombinations(string assetsPath)
        {
            string combinationsPath = System.IO.Path.Combine(assetsPath, "0CraftingCombinations.txt");

            if (!File.Exists(combinationsPath))
            {
                Logger.Warning("Combinations: No '0CraftingCombinations.txt' file not found.");
            }
            else
            {
                List<ModdedCraftingCombo> combinations = new List<ModdedCraftingCombo>();

                try
                {
                    using (StreamReader reader = new StreamReader(combinationsPath))
                    {
                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            string[] splitLine = line.Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);

                            if (splitLine[0] == "name")
                            {
                                CraftingCombination combination = new CraftingCombination();

                                combination.Name = splitLine[1];
                                bool isNewItem = bool.Parse(DeserializeValue(reader));

                                if (SetupCombination(isNewItem, combination, reader, out ModdedCraftingCombo newCombo))
                                {
                                    combinations.Add(newCombo);
                                }
                            }
                        }

                        foreach (ModdedCraftingCombo moddedCraftingCombo in combinations)
                        {
                            _moddedCraftingCombos.Add(moddedCraftingCombo);
                        }

                        combinations.Clear();
                        combinations = null;
                        Logger.Log("Combinations: OK");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Exception($"Skipping crafting combinations file of path '{combinationsPath}' because of error:\n {ex}");
                    combinations.Clear();
                    combinations = null;
                }
            }
        }

        private static void ProcessPrefabInfo(string assetsPath)
        {
            string prefabInfoPath = System.IO.Path.Combine(assetsPath, "0PrefabInfo.txt");
            if (File.Exists(prefabInfoPath))
            {
                using (StreamReader reader = new StreamReader(prefabInfoPath))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries)[0] == "=DescriptionAttributes=")
                        {
                            Logger.Log("Prefabs: Found description attributes section.");
                            InitializeDescriptionAttributes(reader);
                        }
                        else if (line.Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries)[0] == "=Prefabs=")
                        {
                            Logger.Log("Prefabs: Found prefabs section.");
                            InitializePrefabs(reader);
                        }
                    }
                }

                Logger.Log("Prefabs: OK");
            }
            else
            {
                Logger.Warning("Prefabs: No '0PrefabInfo.txt' file not found.");
            }
        }

        private static void InitializePrefabs(StreamReader reader)
        {
            string readLine;

            while ((readLine = reader.ReadLine()) != "}}};")
            {
                if (readLine == "};")
                {
                    GameObject prefab = PrefabFactory.CreateModdedObject(reader, out uint id);

                    if (prefab != null)
                    {
                        if (_moddedPrefabs.ContainsKey(id) || _nameToId.ContainsKey(prefab.name))
                        {
                            Logger.Warning($"Prefabs: Item with id [{id}] OR name [{prefab.name}] already exists.");
                        }
                        else
                        {
                            _moddedPrefabs.Add(id, prefab);
                            _nameToId.Add(prefab.name, id);
                        }
                    }
                }
            }
        }

        private static void InitializeDescriptionAttributes(StreamReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != "}};")
            {
                string[] splitLine = line.Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);

                if (splitLine[0] == "type")
                {
                    int number = int.Parse(DeserializeValue(reader));
                    string description = DeserializeValue(reader);

                    if (splitLine[1] == "attribute")
                    {
                        if (Main.CustomDescriptionAttributes.ContainsKey((AttributeType)number)) Logger.Warning($"Custom description attributes: AttributeType of '{number}' already exists.");
                        else Main.CustomDescriptionAttributes.Add((AttributeType)number, new DescriptionAttribute(description));
                    }
                    else if (splitLine[1] == "interactive")
                    {
                        if (Main.CustomDescriptionAttributes.ContainsKey((InteractiveType)number)) Logger.Warning($"Custom description attributes: InteractiveType of '{number}' already exists.");
                        else Main.CustomDescriptionAttributes.Add((InteractiveType)number, new DescriptionAttribute(description));
                    }
                    else
                    {
                        Logger.Warning($"Custom description attributes: Invalid type read. Expected 'attribute' or 'interactive', found '{splitLine[1]}'.");
                    }
                }
            }
        }

        private static bool SetupCombination(bool isNew, CraftingCombination craftingCombination, StreamReader reader, out ModdedCraftingCombo result)
        {
            try
            {
                if (isNew)
                {
                    craftingCombination.Term = DeserializeValue(reader);

                    foreach (Sprite sprite in _mediumIcons)
                    {
                        if (sprite.name == craftingCombination.Name)
                        {
                            craftingCombination.Sprite = sprite;
                            break;
                        }
                    }

                    craftingCombination.SpriteAssetPath = craftingCombination.Name;
                }
                else
                {
                    craftingCombination.Term = DeserializeValue(reader);

                    craftingCombination.PrefabAssetPath = DeserializeValue(reader);
                    craftingCombination.SpriteAssetPath = DeserializeValue(reader);
                    craftingCombination.Sprite = Resources.Load<Sprite>(craftingCombination.SpriteAssetPath);
                }

                bool unlocked = bool.Parse(DeserializeValue(reader));

                craftingCombination.Lockable = !unlocked;
                craftingCombination.Unlocked = unlocked;

                craftingCombination.IsDynamic = false;

                craftingCombination.Limit = int.Parse(DeserializeValue(reader));
                craftingCombination.IsExtension = bool.Parse(DeserializeValue(reader));

                craftingCombination.CraftsmanshipLevelRequired = int.Parse(DeserializeValue(reader));
                craftingCombination.ExpRewardOnCraft = float.Parse(DeserializeValue(reader));

                string[] resultCraftingType = DeserializeAll(reader);
                craftingCombination.CraftingType = new CraftingType((AttributeType)int.Parse(resultCraftingType[1]), (InteractiveType)int.Parse(resultCraftingType[2]));

                int n = int.Parse(DeserializeValue(reader));

                for (int i = 0; i < n; i++)
                {
                    string[] materialCraftingType = DeserializeAll(reader);
                    CraftingType material = new CraftingType((AttributeType)int.Parse(materialCraftingType[1]), (InteractiveType)int.Parse(materialCraftingType[2]));

                    craftingCombination.Materials.Add(material);
                }

                string subcategoryName = DeserializeValue(reader);
                string categoryName = DeserializeValue(reader);

                result = new ModdedCraftingCombo(categoryName, subcategoryName, craftingCombination);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Exception($"Skipping crafting combination of name '{craftingCombination.Name}' because of error:\n {ex}");
                result = null;
                return false;
            }
        }

        internal static void InjectCombinations()
        {
            CraftingCombinations craftingCombinations = PlayerRegistry.LocalPlayer.Crafter.CraftingCombinations;

            foreach (ModdedCraftingCombo combo in _moddedCraftingCombos)
            {
                CraftingCategory category = null;
                CraftingSubcategory subcategory = null;

                foreach (CraftingCategory pcat in craftingCombinations.Categories)
                {
                    if (combo.Category == pcat.Name)
                    {
                        category = pcat;
                        break;
                    }
                }

                if (category == null)
                {
                    category = new CraftingCategory();
                    category.Name = combo.Category;
                    category.Term = combo.Category;
                    craftingCombinations.Categories.Add(category);
                }

                foreach (CraftingSubcategory psubcat in category.Subcategories)
                {
                    if (combo.Subcategory == psubcat.Name)
                    {
                        subcategory = psubcat;
                        break;
                    }
                }

                if (subcategory == null)
                {
                    subcategory = new CraftingSubcategory();
                    subcategory.Name = combo.Subcategory;
                    subcategory.Term = combo.Subcategory;
                    category.Subcategories.Add(subcategory);
                }

                subcategory.Combinations.Add(combo.Combination);
                craftingCombinations.Combinations.Add(combo.Combination);
            }

            Logger.Log("Combination injection: Passed.");
        }

        internal static void InjectIcons()
        {
            if (_iconsLoaded) return;

            FieldInfo fi_smallIcons = typeof(Icons).GetField("_smallIcons", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo fi_mediumIcons = typeof(Icons).GetField("_mediumIcons", BindingFlags.Static | BindingFlags.NonPublic);

            Dictionary<IconKey, Sprite> otherspritesS;
            Dictionary<IconKey, Sprite> otherspritesM;

            otherspritesS = (Dictionary<IconKey, Sprite>)fi_smallIcons.GetValue(null);
            otherspritesM = (Dictionary<IconKey, Sprite>)fi_mediumIcons.GetValue(null);

            foreach (Sprite sprite in _smallIcons)
            {
                try
                {
                    CraftingType craftingType = GetModdedPrefab(sprite.name).GetComponent<BaseObject>().CraftingType;
                    otherspritesS.Add(new IconKey { CraftingType = craftingType, CustomType = ECustomType.None }, sprite);
                }
                catch (Exception ex)
                {
                    Logger.Exception($"Error while overloading small icon [{sprite.name}]: \n {ex}");
                }
            }

            foreach (Sprite sprite in _mediumIcons)
            {
                try
                {
                    CraftingType craftingType = GetModdedPrefab(sprite.name).GetComponent<BaseObject>().CraftingType;
                    otherspritesM.Add(new IconKey { CraftingType = craftingType, CustomType = ECustomType.None }, sprite);
                }
                catch (Exception ex)
                {
                    Logger.Exception($"Error while overloading medium icon [{sprite.name}]: \n {ex}");
                }
            }

            //add small meat icon
            try { otherspritesS.Add(new IconKey { CraftingType = new CraftingType(AttributeType.Small, InteractiveType.FOOD_MEAT), CustomType = ECustomType.None }, GetSmallMeatIcon()); }
            catch { Logger.Exception("Cannot readd small meat icon."); }
            //

            fi_smallIcons.SetValue(null, otherspritesS);
            fi_mediumIcons.SetValue(null, otherspritesM);

            Logger.Log("Inject icons: Passed.");
            _iconsLoaded = true;
        }

        private static Sprite GetSmallMeatIcon()
        {
            Texture2D texture2D = new Texture2D(128, 128, TextureFormat.ARGB32, false);
            texture2D.LoadImage(CatUtility.ExtractEmbeddedResource("SDPublicFramework.FOOD_MEAT-SMALL.png", Assembly.GetExecutingAssembly()));
            return Sprite.Create(texture2D, new Rect(0f, 0f, 128f, 128f), new Vector2(0, 0));
        }

        //remove this and add override feature instead?
        internal static void ProcessDrops()
        {
            CreateVanillaLoot(8f);
            CreateVanillaLoot(0.5f, "Prefabs/StrandedObjects/Tools/FISHING_ROD");
            CreateVanillaLoot(0.5f, "Prefabs/StrandedObjects/Tools/BOBBER");
            CreateVanillaLoot(1.5f, "Prefabs/StrandedObjects/Crafting/BUOYBALL");
            CreateVanillaLoot(1f, "Prefabs/StrandedObjects/Crafting/ROCK");
            CreateVanillaLoot(4f, "Prefabs/StrandedObjects/Crafting/LEAVES_FIBROUS");
            CreateVanillaLoot(2.5f, "Prefabs/StrandedObjects/Crafting/PALM_FROND");
            CreateVanillaLoot(0.5f, "Prefabs/StrandedObjects/Crafting/PART_GYRO");
            CreateVanillaLoot(0.5f, "Prefabs/StrandedObjects/EndGameRewards/NEW_SPEARGUN_CARBON_ARROW");

            Logger.Log("Vanilla drops: Passed.");
        }

        public static LootSpawn.LootItem CreateVanillaLoot(float weight, string resourcePath = "null", bool addToList = true)
        {
            LootSpawn.LootItem lootItemBasic = new LootSpawn.LootItem();

            lootItemBasic.Weight = weight;
            lootItemBasic.Item = resourcePath == "null" ? null : Resources.Load<GameObject>(resourcePath);

            if (addToList)
            {
                _containerItems.Add(lootItemBasic);
            }

            return lootItemBasic;
        }

        internal static IConsumableSound GetConsumableSound(uint itemId)
        {
            if (_customClips.ContainsKey(itemId)) return _customClips[itemId] as IConsumableSound;
            return null;
        }

        /// <summary> For reading 0.txt file info </summary>
        public static string[] DeserializeAll(StreamReader reader)
        {
            string[] readValues = reader.ReadLine().Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);
            Logger.Debugger($"Trying to deserialize values of name: '{readValues[0]}'");

            return readValues;
        }

        /// <summary> For reading 0.txt file info </summary>
        public static string DeserializeValue(StreamReader reader, int i = 1)
        {
            string[] readValues = reader.ReadLine().Split(new string[] { ":", ";" }, StringSplitOptions.RemoveEmptyEntries);
            Logger.Debugger($"Trying to deserialize value of name: '{readValues[0]}'");

            return readValues[i];
        }

        private static List<string> _registeredPaths = new List<string>();
        private static Dictionary<uint, Type> _customTypes = new Dictionary<uint, Type>();

        private static List<ModdedCraftingCombo> _moddedCraftingCombos = new List<ModdedCraftingCombo>();

        private static Dictionary<uint, GameObject> _moddedPrefabs = new Dictionary<uint, GameObject>();
        private static Dictionary<string, uint> _nameToId = new Dictionary<string, uint>();
        private static Dictionary<uint, CustomSound> _customClips = new Dictionary<uint, CustomSound>();

        private static List<AssetBundle> _assetBundles = new List<AssetBundle>();

        private static Dictionary<string, Func<BaseObject, CraftingCombination, Crafter, IPlayer, IList<IBase>, bool>> _craftingLogic =
            new Dictionary<string, Func<BaseObject, CraftingCombination, Crafter, IPlayer, IList<IBase>, bool>>();

        private static List<Sprite> _subcategoryIcons = new List<Sprite>();
        private static List<Sprite> _smallIcons = new List<Sprite>();
        private static List<Sprite> _mediumIcons = new List<Sprite>();

        private static Dictionary<uint, float> _moddedContainerItems = new Dictionary<uint, float>();
        private static List<LootSpawn.LootItem> _containerItems = new List<LootSpawn.LootItem>();

        private static bool _iconsLoaded = false;
        private static bool _frameworkInitialized = false;
        private static byte _bundleCount = 0;

        private enum ModdedIconType
        {
            SUBCATEGORY,
            SMALL,
            MEDIUM
        }

        public const string MODDED_ITEM_NAME_START = "mod_";
    }
}