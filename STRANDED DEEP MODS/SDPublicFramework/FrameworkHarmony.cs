using System;
using Beam;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using HarmonyLib;
using Beam.Crafting;
using Funlabs;
using Beam.Serialization;
using Beam.Serialization.Json;
using Beam.UI;
using Beam.Terrain;
using Beam.Utilities;
using Beam.Events;
using System.Linq;
using UnityModManagerNet;

namespace SDPublicFramework
{
    //for loading registered mods
    [HarmonyPatch(typeof(UnityModManager.Logger), nameof(UnityModManager.Logger.Log), new Type[] {typeof(string)})]
    class UnityModManager_Logger_Log_Patch
    {
        private static void Postfix(string str)
        {
            if (str.StartsWith("FINISH."))
            {
                Framework.InjectModsToFramework();
            }
        }
    }

    //for icons and recipes injection there needs to be an event trigger for when player is ready
    [HarmonyPatch(typeof(Player), nameof(Player.Initialize))]
    class Player_Initialize_Patch
    {
        private static void Postfix(Player __instance)
        {
            Logger.Log($"Detected [Player.Initialize] of player id '{__instance.Id}'. Attempting final injection.");
            if (__instance.Id == 0 && first)
            {
                Framework.InjectIcons();
                Framework.InjectCombinations();
                EquippableSystem.Initialize();

                Logger.Log("Final injection done.");
            }
            else
            {
                Logger.Log($"Attempt stopped [f:{first}].");
            }

            first = !first;
        }

        private static bool first = true;
    }

    //loading sprites
    [HarmonyPatch(typeof(SpriteDictionary), nameof(SpriteDictionary.GetSprite), new Type[] { typeof(string) })]
    class SpriteDictionary_GetSprite_Patch
    {
        private static void Postfix(ref Sprite __result, string spriteName)
        {
            foreach (Sprite sprite in Framework.SubcategoryIcons)
            {
                if (spriteName == sprite.name)
                {
                    __result = sprite;
                }
            }
        }
    }

    //loading prefabs
    [HarmonyPatch(typeof(SavingUtilities), nameof(SavingUtilities.LoadPrefabFromJData), new Type[] { typeof(MultiplayerZoneSync.Usage), typeof(JObject), typeof(Action<SaveablePrefab>), typeof(Action<SaveablePrefab>) })]
    class SavingUtilities_LoadPrefabFromJData_Patch
    {
        private static bool Prefix(MultiplayerZoneSync.Usage usage, JObject prefabData, Action<SaveablePrefab> preload = null, Action<SaveablePrefab> postLoad = null)
        {
            uint prefabId = Prefabs.GetPrefabId(prefabData); //sitas veikia tik inventoriuj
            if (prefabId > 400U)
            {
                MiniGuid referenceId = Prefabs.GetReferenceId(prefabData, true);

                if (Game.Mode.IsMaster() || !Prefabs.IsMultiplayerEntity(prefabId))
                {
                    BaseObject baseObject = PrefabFactory.InstantiateModdedObject(prefabId);

                    baseObject.ReferenceId = referenceId;

                    AccessTools.Method(typeof(SavingUtilities), "Load", new Type[] { typeof(SaveablePrefab), typeof(JObject), typeof(Action<SaveablePrefab>), typeof(Action<SaveablePrefab>) }).Invoke(null, new object[] { baseObject as SaveablePrefab, prefabData, preload, postLoad });
                    baseObject.gameObject.SetActive(true);
                    return false;
                }
                MultiplayerZoneSync.Instance.LinkData(usage, referenceId, delegate (MultiplayerZoneSync.Command command)
                {
                    AccessTools.Method(typeof(SavingUtilities), "Load", new Type[] { typeof(SaveablePrefab), typeof(JObject), typeof(Action<SaveablePrefab>), typeof(Action<SaveablePrefab>) }).Invoke(null, new object[] { command.Instance, prefabData, preload, postLoad });
                }, null, null);

                return false;
            }
            else return true;
        }
    }

    [HarmonyPatch(typeof(ZoneLoader), "CreateLoadedPrefab", new Type[] { typeof(JObject), typeof(Zone) })]
    class ZoneLoader_CreateLoadedPrefab_Patch
    {
        private static readonly AccessTools.FieldRef<ZoneLoader, Vector3> _positionRef = AccessTools.FieldRefAccess<ZoneLoader, Vector3>("_position");

        private static bool Prefix(Beam.Terrain.ZoneLoader __instance, JObject data, Zone zone)
        {
            JObject field = data.GetField("prefab");
            if (field == null)
            {
                UnityEngine.Debug.LogError("ZoneLoader::LoadZoneAsync:: Error loading. No prefab id field found for object data.");
                return false;
            }
            uint prefabId = Prefabs.ConvertLegacyPrefabId(field);
            if (prefabId == 0U)
            {
                UnityEngine.Debug.LogError("ZoneLoader::LoadZoneAsync:: Error loading instantiated object with empty prefab id.");
                return false;
            }
            if (prefabId > 400U)
            {
                AccessTools.Method(typeof(ZoneLoader), "Load", new Type[] { typeof(JObject), typeof(Vector3), typeof(Zone), typeof(SaveablePrefab) }).Invoke(__instance, new object[] { data, _positionRef(__instance), zone, PrefabFactory.InstantiateModdedObject(prefabId).GetComponent<SaveablePrefab>() });
                _positionRef(__instance) += new Vector3(0f, 0f, 50f);
            }
            else
            {
                return true;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(ZoneLoader), "CreateGeneratedPrefab", new Type[] { typeof(GeneratedObject), typeof(Zone) })]
    class ZoneLoader_CreateGeneratedPrefab_Patch
    {
        [HarmonyBefore(new string[] { "StrandedWideMod" })]
        private static bool Prefix(GeneratedObject objectData, Zone zone)
        {
            if (Main.StrandedWideOn) return true;

            return CreatePrefab(objectData, zone);
        }

        private static void Postfix(GeneratedObject objectData, Zone zone)
        {
            if (Main.StrandedWideOn)
            {
                CreatePrefab(objectData, zone);
            }
        }

        private static bool CreatePrefab(GeneratedObject objectData, Zone zone)
        {
            SaveablePrefab component = objectData.Prefab.GetComponent<SaveablePrefab>();
            GameObject gameObject;

            if (component != null)
            {
                uint prefabId = component.PrefabId;
                if (prefabId > 400U)
                {
                    MiniGuid referenceId = MiniGuid.NewFrom(objectData.Position, prefabId, 48879);
                    gameObject = PrefabFactory.InstantiateModdedObject(prefabId).gameObject;
                    gameObject.GetComponent<SaveablePrefab>().ReferenceId = referenceId;
                    gameObject.transform.SetParent(zone.SaveContainer, false);
                    gameObject.transform.position = objectData.Position;
                    gameObject.transform.rotation = objectData.Rotation;
                }
                else return true;
            }
            else
            {
                gameObject = UnityEngine.Object.Instantiate<GameObject>(objectData.Prefab, objectData.Position, objectData.Rotation);
            }
            gameObject.transform.SetParent(zone.SaveContainer, false);
            gameObject.transform.position = objectData.Position;
            gameObject.transform.rotation = objectData.Rotation;

            zone.OnObjectCreated(gameObject);

            return false;
        }
    }

    [HarmonyPatch(typeof(Prefabs), nameof(Prefabs.GetPrefab), new Type[] { typeof(uint) })]
    class Prefabs_GetPrefab_Patch
    {
        private static bool Prefix(ref SaveablePrefab __result, uint id)
        {
            if (id > 400U)
            {
                return false;
            }

            return true;
        }
    }

    //Spawning loot in containers
    [HarmonyPatch(typeof(LootSpawn), nameof(LootSpawn.ChooseItem), new Type[] { typeof(LootSpawn.LootItem[]), typeof(float) })]
    class LootSpawn_ChooseItem_Patch
    {
        private static void InitializeLoot(LootSpawn.LootItem[] items)
        {
            _modifiedList = new List<LootSpawn.LootItem>();

            foreach (LootSpawn.LootItem lootItem in items)
            {
                if (lootItem.Item == null) lootItem.Weight = 35f;
                else if (lootItem.Item.name == "FLARE_GUN") lootItem.Weight = 8f;
                else if (lootItem.Item.name == "DUCTTAPE") lootItem.Weight = 5f;
                else if (lootItem.Item.name == "NEW_REFINED_HAMMER") lootItem.Weight = 5f;
                else if (lootItem.Item.name == "TORCH") lootItem.Weight = 8f;
                else if (lootItem.Item.name == "PART_ENGINE") lootItem.Weight = 3f;
                else if (lootItem.Item.name == "PART_FUEL") lootItem.Weight = 3f;
                else if (lootItem.Item.name == "PART_ELECTRICAL") lootItem.Weight = 3f;
                else if (lootItem.Item.name == "PART_FILTER") lootItem.Weight = 3f;
                else if (lootItem.Item.name == "CAN_BEANS") lootItem.Weight = 6f;
                else if (lootItem.Item.name == "LEATHER") lootItem.Weight = 7f;
                else if (lootItem.Item.name == "CLOTH") lootItem.Weight = 12f;
                else if (lootItem.Item.name == "SCRAP_PLANK") lootItem.Weight = 11f;
                else if (lootItem.Item.name == "NEW_FISHING_SPEAR") lootItem.Weight = 10f;

                _allWeight += lootItem.Weight;
                _modifiedList.Add(lootItem);
            }

            foreach (LootSpawn.LootItem moddedItem in Framework.AdditionalVanillaItemLootTable)
            {
                _allWeight += moddedItem.Weight;
                _modifiedList.Add(moddedItem);
            }

            foreach (KeyValuePair<uint, float> item in Framework.ModdedItemLootTable)
            {
                _allWeight += item.Value;
            }

            _listWasModified = true;
        }

        private static bool Prefix(ref LootSpawn.LootItem __result, LootSpawn.LootItem[] Items, float percentage)
        {
            if (!_listWasModified)
            {
                InitializeLoot(Items);
            }

            float chosenWeight = percentage * _allWeight;

            for (int j = 0; j < _modifiedList.Count + 1; j++)
            {
                if (j != _modifiedList.Count)
                {
                    if (chosenWeight < _modifiedList[j].Weight)
                    {
                        __result = _modifiedList[j];
                        return false;
                    }

                    chosenWeight -= _modifiedList[j].Weight;
                }
                else
                {
                    foreach (KeyValuePair<uint, float> item in Framework.ModdedItemLootTable)
                    {
                        if (chosenWeight < item.Value)
                        {
                            LootSpawn.LootItem lootItem = new LootSpawn.LootItem();
                            lootItem.Weight = item.Value;
                            lootItem.Item = Framework.GetModdedPrefab(item.Key);

                            __result = lootItem;
                            return false;
                        }

                        chosenWeight -= item.Value;
                    }
                }
            }
            __result = _modifiedList[0];
            return false;
        }

        private static bool _listWasModified = false;
        private static float _allWeight = 0f;
        private static List<LootSpawn.LootItem> _modifiedList;
    }

    [HarmonyPatch(typeof(Interactive_STORAGE), "OnSlotStorageOpen")]
    class Interactive_STORAGE_OnSlotStorageOpen_Patch
    {
        private static readonly AccessTools.FieldRef<Interactive_STORAGE, JObject> _storageDataRef = AccessTools.FieldRefAccess<Interactive_STORAGE, JObject>("_storageData");
        private static readonly AccessTools.FieldRef<Interactive_STORAGE, SlotStorage> _slotStorageRef = AccessTools.FieldRefAccess<Interactive_STORAGE, SlotStorage>("_slotStorage");
        private static readonly AccessTools.FieldRef<Interactive_STORAGE, bool> _createdRef = AccessTools.FieldRefAccess<Interactive_STORAGE, bool>("_created");
        private static readonly AccessTools.FieldRef<Interactive_STORAGE, bool> _spawnItemLootRef = AccessTools.FieldRefAccess<Interactive_STORAGE, bool>("_spawnItemLoot");
        private static readonly AccessTools.FieldRef<Interactive_STORAGE, LootSpawn.LootItem[]> _itemLootRef = AccessTools.FieldRefAccess<Interactive_STORAGE, LootSpawn.LootItem[]>("_itemLoot");
        private static readonly AccessTools.FieldRef<Interactive_STORAGE, GameObject[]> _startingLootRef = AccessTools.FieldRefAccess<Interactive_STORAGE, GameObject[]>("_startingLoot");

        private static bool Prefix(Interactive_STORAGE __instance)
        {
            if (_storageDataRef(__instance) != null && !_storageDataRef(__instance).IsNull() && !_slotStorageRef(__instance).LoadState.IsLoaded())
            {
                if (_slotStorageRef(__instance).LoadState.IsIdle())
                {
                    StorageUtilities.LoadStorage(_slotStorageRef(__instance), _storageDataRef(__instance));
                }
            }
            else if (Game.Mode.IsMaster() && !_createdRef(__instance) && _spawnItemLootRef(__instance) && !(bool)typeof(Interactive_STORAGE).GetProperty("LootSpawned", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance))
            {
                typeof(Interactive_STORAGE).GetProperty("LootSpawned", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, true);

                if (_startingLootRef(__instance).Length != 0)
                {
                    for (int i = 0; i < _startingLootRef(__instance).Length; i++)
                    {
                        InteractiveObject interactiveObject = MultiplayerMng.Instantiate<InteractiveObject>(_startingLootRef(__instance)[i].GetComponent<ISaveablePrefab>().PrefabId, MiniGuid.New(), null);
                        if (interactiveObject != null && interactiveObject.CanPickUp)
                        {
                            _slotStorageRef(__instance).ReplicatedPush(interactiveObject);
                        }
                        else
                        {
                            Debug.LogWarning("Interactive Storage :: Failed to generate Item - " + interactiveObject.gameObject.name);
                            MultiplayerMng.Destroy(interactiveObject, DestroyDelay.SAFE);
                        }
                    }
                }
                if (_itemLootRef(__instance).Length != 0)
                {
                    for (int j = 0; j < _slotStorageRef(__instance).SlotCount; j++)
                    {
                        float value = UnityEngine.Random.value;
                        GameObject item = LootSpawn.ChooseItem(_itemLootRef(__instance), value).Item;
                        if (item != null)
                        {
                            if (item.name.StartsWith("mod"))
                            {
                                InteractiveObject interactiveObject2 = PrefabFactory.InstantiateModdedObject(item.name).GetComponent<InteractiveObject>();
                                interactiveObject2.ReferenceId = MiniGuid.New();

                                if (interactiveObject2 != null && interactiveObject2.CanPickUp)
                                {
                                    _slotStorageRef(__instance).ReplicatedPush(interactiveObject2);
                                }
                                else
                                {
                                    Logger.Warning($"Failed to generate modded item of name [{interactiveObject2.gameObject.name}]. Destroying object.");
                                    MultiplayerMng.Destroy(interactiveObject2, DestroyDelay.SAFE);
                                }
                            }
                            else
                            {
                                InteractiveObject interactiveObject2 = MultiplayerMng.Instantiate<InteractiveObject>(item.GetComponent<ISaveablePrefab>().PrefabId, MiniGuid.New(), null);
                                if (interactiveObject2 != null && interactiveObject2.CanPickUp)
                                {
                                    _slotStorageRef(__instance).ReplicatedPush(interactiveObject2);
                                }
                                else
                                {
                                    Debug.LogWarning("Interactive Storage :: Failed to generate Item - " + interactiveObject2.gameObject.name);
                                    MultiplayerMng.Destroy(interactiveObject2, DestroyDelay.SAFE);
                                }
                            }
                        }
                    }
                }
            }

            AccessTools.Method(typeof(Interactive_STORAGE), "OnOpened", new Type[] { typeof(Interactive_STORAGE) }).Invoke(__instance, new object[] { __instance });
            return false;
        }
    }

    //food restores health
    [HarmonyPatch(typeof(Statistics), nameof(Statistics.Eat), new Type[] { typeof(InteractiveType), typeof(MeatProvenance), typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(float), typeof(bool) })]
    class Statistics_UpdateFoods_InvokeRepeating_Patch
    {
        private static readonly AccessTools.FieldRef<Statistics, IPlayer> _playerRef = AccessTools.FieldRefAccess<Statistics, IPlayer>("_player");
        private static readonly AccessTools.FieldRef<Statistics, float> _healthRef = AccessTools.FieldRefAccess<Statistics, float>("_health");

        private static void Postfix(Statistics __instance, InteractiveType type, MeatProvenance provenance, int calorieGain, int fibresGain, int fatGain, int proteinsGain, float fluidGain = 0f, float vomitChance = 0f, bool canCauseDiarrhoea = false)
        {
            calorieGain = (int)((float)calorieGain * _playerRef(__instance).PlayerSkills.CookingMultiplier);
            _healthRef(__instance) += ((float)calorieGain * 0.04f);
        }
    }

    //NO ICON ON PINNED MODDED ITEM RECIPE FIX
    [HarmonyPatch(typeof(PinnedComboView), "SetSprite", new Type[] { typeof(string) })]
    class PinnedComboView_SetSprite_Patch
    {
        private static readonly AccessTools.FieldRef<PinnedComboView, UImageViewAdapter> _comboImageRef = AccessTools.FieldRefAccess<PinnedComboView, UImageViewAdapter>("_comboImage");

        private static bool Prefix(string spriteAssetPath, PinnedComboView __instance)
        {
            if (spriteAssetPath.StartsWith(Framework.MODDED_ITEM_NAME_START))
            {
                foreach (Sprite sprite in Framework.MediumIcons)
                {
                    if (spriteAssetPath == sprite.name)
                    {
                        _comboImageRef(__instance).Sprite = sprite;
                        break;
                    }
                }

                return false;
            }

            return true;
        }
    }

    //handle sound effects of medical items
    [HarmonyPatch(typeof(InteractiveObject_MEDICAL), nameof(InteractiveObject_MEDICAL.ValidatePrimary), new Type[] { typeof(IBase) })]
    class InteractiveObject_MEDICAL_ValidatePrimary_Patch
    {
        private static bool Prefix(InteractiveObject_MEDICAL __instance, ref bool __result, IBase obj)
        {
            if (__instance.PrefabId >= 400U)
            {
                if (__instance.CraftingType.InteractiveType == InteractiveType.MEDICAL_ANTIDOTE)
                {
                    __result = CheckConsumable(__instance, StatusEffect.POISON, false);
                }
                else if (__instance.CraftingType.InteractiveType == InteractiveType.MEDICAL_BANDAGE || __instance.CraftingType.InteractiveType == InteractiveType.MEDICAL_GAUZE)
                {
                    __result = CheckConsumable(__instance, StatusEffect.BLEEDING, false);
                }
                else if (__instance.CraftingType.InteractiveType == InteractiveType.MEDICAL_AJUGA)
                {
                    __result = CheckConsumable(__instance, StatusEffect.BOOST_BREATH, true);
                }
                else if (__instance.CraftingType.InteractiveType == InteractiveType.MEDICAL_SPLINT)
                {
                    __result = CheckConsumable(__instance, StatusEffect.BROKEN_BONES, false);
                }
                else if (__instance.CraftingType.InteractiveType == InteractiveType.MEDICAL_ALOE_SALVE)
                {
                    __result = CheckConsumable(__instance, StatusEffect.SUNBLOCK, true);
                }
                //if it's a custom medicament, handle it yourself

                return false;
            }

            return true;
        }

        private static bool CheckConsumable(InteractiveObject_MEDICAL item, PlayerEffect effect, bool add)
        {
            IPlayer user = PlayerUtilities.GetPlayerFromId(item.OwnerId);

            if (user != null && item.DurabilityPoints > 0)
            {
                IConsumableSound consumableSound = item as IConsumableSound;
                if (consumableSound == null) { consumableSound = Framework.GetConsumableSound(item.PrefabId); }

                if (add)
                {
                    if (user.Statistics.HasStatusEffect(effect))
                    {
                        return false;
                    }

                    HandleMedicalConsumableSound(consumableSound);
                    return user.Statistics.ApplyStatusEffect(effect);
                }
                else
                {
                    if (!user.Statistics.HasStatusEffect(effect))
                    {
                        return false;
                    }

                    HandleMedicalConsumableSound(consumableSound);
                    return user.Statistics.RemoveStatusEffect(effect);
                }
            }

            return false;
        }

        private static void HandleMedicalConsumableSound(IConsumableSound customSound)
        {
            if (customSound != null)
            {
                if (customSound.ConsumableClip == null) Logger.Exception("This medicine consume sound doesn't exist.");
                AudioManager.GetAudioPlayer().Play2D(customSound.ConsumableClip, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else AudioManager.GetAudioPlayer().Play2D(BasicSounds.DrinkSound, AudioMixerChannels.Voice, AudioPlayMode.Single);
        }
    }

    //handle medical item refund of modded items
    [HarmonyPatch(typeof(InteractiveObject_MEDICAL), nameof(InteractiveObject_MEDICAL.Destroy))]
    class InteractiveObject_MEDICAL_Destroy_Patch
    {
        private static readonly AccessTools.FieldRef<InteractiveObject, IPlayer> _ownerRef = AccessTools.FieldRefAccess<InteractiveObject, IPlayer>("_owner");
        private static readonly AccessTools.FieldRef<InteractiveObject_MEDICAL, uint> _refundPrefabIdRef = AccessTools.FieldRefAccess<InteractiveObject_MEDICAL, uint>("_refundPrefabId");

        private static void Postfix(InteractiveObject_MEDICAL __instance)
        {
            if (__instance.PrefabId > 400U)
            {
                try
                {
                    uint refundId = _refundPrefabIdRef(__instance);
                    if (refundId != 0U)
                    {
                        if (refundId > 400U)
                        {
                            BaseObject moddedObject = PrefabFactory.InstantiateModdedObject(refundId);
                            InteractiveObject_FOOD foodComponent = moddedObject as InteractiveObject_FOOD;

                            if (foodComponent != null)
                            {
                                foodComponent.Servings = 0;
                            }

                            AccessTools.Method(typeof(InteractiveObject_MEDICAL), "ReplicatedPickupEmptyRefund", new Type[] { typeof(IPlayer), typeof(IPickupable) })
                                .Invoke(__instance, new object[] { _ownerRef(__instance), foodComponent, null});
                        }
                        else
                        {
                            AccessTools.Method(typeof(InteractiveObject_MEDICAL), "ReplicatedPickupEmptyRefund", new Type[] { typeof(IPlayer), typeof(IPickupable) })
                                .Invoke(__instance, new object[] { _ownerRef(__instance), (IPickupable)MultiplayerMng.Instantiate<SaveablePrefab>(refundId, MiniGuid.New(), null) });
                        }
                    }
                }
                catch(Exception ex) { Debug.Log(ex); }
            }
        }
    }

    //handle sound effects of food items
    [HarmonyPatch(typeof(InteractiveObject_FOOD), nameof(InteractiveObject_FOOD.ValidatePrimary), new Type[] { typeof(IBase) })]
    class InteractiveObject_FOOD_ValidatePrimary_Patch
    {
        private static readonly AccessTools.FieldRef<InteractiveObject, IPlayer> _ownerRef = AccessTools.FieldRefAccess<InteractiveObject, IPlayer>("_owner");

        private static bool Prefix(InteractiveObject_FOOD __instance, ref bool __result, IBase obj)
        {
            if (__instance.PrefabId > 400U)
            {
                IPlayer user = PlayerUtilities.GetPlayerFromId(__instance.OwnerId);

                bool isWater = __instance.Hydration > __instance.Calories;
                float fullnessPercentage = isWater ? user.Statistics.FluidsPercent : user.Statistics.CaloriesPercent;
                __result = false;

                if (__instance.Servings > 0 && fullnessPercentage < 0.98f)
                {
                    __result = true;

                    IConsumableSound consumableSound = __instance as IConsumableSound;
                    if (consumableSound == null) { consumableSound = Framework.GetConsumableSound(__instance.PrefabId); }

                    HandleFoodConsumableSound(isWater, consumableSound);
                }

                return false;
            }

            return true;
        }

        private static void HandleFoodConsumableSound(bool isWater, IConsumableSound customSound)
        {
            if (customSound != null)
            {
                if (customSound.ConsumableClip == null) Logger.Exception("This food consume sound doesn't exist.");
                AudioManager.GetAudioPlayer().Play2D(customSound.ConsumableClip, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (isWater) AudioManager.GetAudioPlayer().Play2D(BasicSounds.DrinkSound, AudioMixerChannels.Voice, AudioPlayMode.Single);
            else AudioManager.GetAudioPlayer().Play2D(BasicSounds.EatSound, AudioMixerChannels.Voice, AudioPlayMode.Single);
        }
    }

    //crafting
    [HarmonyPatch(typeof(Crafter), nameof(Crafter.Craft), new Type[] { typeof(CraftingCombination), typeof(MiniGuid) })]
    class Crafter_Craft_Patch
    {
        private static readonly AccessTools.FieldRef<Crafter, CraftingCombination> _currentCombinationRef = AccessTools.FieldRefAccess<Crafter, CraftingCombination>("_currentCombination");
        private static readonly AccessTools.FieldRef<Crafter, Dictionary<CraftingType, IList<IBase>>> _cachedMaterialsLookupRef = AccessTools.FieldRefAccess<Crafter, Dictionary<CraftingType, IList<IBase>>>("_cachedMaterialsLookup");
        private static readonly AccessTools.FieldRef<Crafter, IList<IBase>> _backupCraftedMaterialsRef = AccessTools.FieldRefAccess<Crafter, IList<IBase>>("_backupCraftedMaterials");
        private static readonly AccessTools.FieldRef<Crafter, bool> _executingDelayedPlaceActionRef = AccessTools.FieldRefAccess<Crafter, bool>("_executingDelayedPlaceAction");
        private static readonly AccessTools.FieldRef<InteractiveObject_FUELCAN, float> _fuelRef = AccessTools.FieldRefAccess<InteractiveObject_FUELCAN, float>("_fuel");
        private static readonly AccessTools.FieldRef<Crafter, IPlayer> _playerRef = AccessTools.FieldRefAccess<Crafter, IPlayer>("_player");

        private static bool Prefix(Crafter __instance, CraftingCombination combination, MiniGuid referenceId)
        {
            bool modded = combination.Name.StartsWith(Framework.MODDED_ITEM_NAME_START);
            bool regular = combination.Name.StartsWith("regular_");

            if (modded || regular)
            {
                _currentCombinationRef(__instance) = combination;
                _backupCraftedMaterialsRef(__instance).Clear();

                foreach (CraftingType craftingType in _currentCombinationRef(__instance).Materials.Skip(_currentCombinationRef(__instance).IsExtension ? 1 : 0))
                {
                    IList<IBase> list;
                    if (_cachedMaterialsLookupRef(__instance).TryGetValue(craftingType, out list))
                    {
                        int num = list.Count - 1;
                        if (num >= 0)
                        {
                            IBase item = list[num];
                            list.Remove(item);
                            _backupCraftedMaterialsRef(__instance).Add(item);
                        }
                    }
                }

                if (!_executingDelayedPlaceActionRef(__instance))
                {
                    if (_playerRef(__instance).IsOwner)
                    {
                        AccessTools.Method(typeof(Crafter), "DisableCraftingInput").Invoke(__instance, new object[] { true });
                    }

                    BeamTiming.WaitAnd((float)AccessTools.Method(typeof(Crafter), "GetCraftingDelay").Invoke(__instance, new object[] { }), delegate
                    {
                        if (_playerRef(__instance).IsOwner)
                        {
                            AccessTools.Method(typeof(Crafter), "DisableCraftingInput").Invoke(__instance, new object[] { false });
                        }
                        _executingDelayedPlaceActionRef(__instance) = false;

                        if ((bool)AccessTools.Method(typeof(Crafter), "AreAllMaterialsAvailable").Invoke(__instance, new object[] { }) || (bool)AccessTools.Method(typeof(Crafter), "IsCraftingFree").Invoke(__instance, new object[] { }))
                        {
                            if (!_backupCraftedMaterialsRef(__instance).IsNullOrDestroyed())
                            {
                                BaseObject baseObject;

                                if (modded) baseObject = PrefabFactory.InstantiateModdedObject(combination.Name).gameObject.GetComponent<BaseObject>();
                                else baseObject = MultiplayerMng.Instantiate<BaseObject>(_currentCombinationRef(__instance).PrefabAssetPath, referenceId);

                                if (Framework.CraftingLogic.ContainsKey(combination.Name))
                                {
                                    if (!Framework.CraftingLogic[combination.Name](baseObject, combination, __instance, _playerRef(__instance), _backupCraftedMaterialsRef(__instance))) return;
                                }

                                if (modded)
                                {
                                    baseObject.gameObject.SetActive(true);
                                    baseObject.ReferenceId = new MiniGuid();

                                    InteractiveObject interactiveObject = baseObject as InteractiveObject;
                                    interactiveObject.EnablePhysics();

                                    if (_playerRef(__instance).Holder.CurrentObject == null) _playerRef(__instance).Holder.Pickup(interactiveObject, false);
                                    else if (_playerRef(__instance).Inventory.GetSlotStorage().CanPush(interactiveObject))
                                    {
                                        _playerRef(__instance).Inventory.GetSlotStorage().Push(interactiveObject);
                                        CatUtility.PopNotification(baseObject.DisplayName + " stored in the inventory.", 4.5f);
                                    }
                                    else
                                    {
                                        baseObject.gameObject.transform.position = _playerRef(__instance).transform.position;
                                        baseObject.gameObject.transform.rotation = _playerRef(__instance).transform.rotation;
                                        CatUtility.PopNotification("No space in inventory. " + baseObject.DisplayName + " dropped on ground instead.", 5f);
                                    }

                                    EventManager.RaiseEvent<ExperienceEvent>(new ExperienceEvent(_playerRef(__instance).Id, PlayerSkills.Skills.CRAFTSMANSHIP, _currentCombinationRef(__instance).ExpRewardOnCraft));
                                    AccessTools.Method(typeof(Crafter), "DestroyBackupMaterials").Invoke(__instance, new object[] { });
                                    AccessTools.Method(typeof(Crafter), "CleanupCachedMaterials").Invoke(__instance, new object[] { });
                                }
                                else
                                {
                                    AccessTools.Method(typeof(Crafter), "Craft", new Type[] { typeof(SaveablePrefab) }).Invoke(__instance, new object[] { baseObject });
                                }
                            }
                        }
                    });
                    _executingDelayedPlaceActionRef(__instance) = true;
                }
                return false;
            }
            return true;
        }
    }

    /************************************ FOR MULTIPLAYER ****************************************/
    //which i gave up on. might be useful later


    /*[HarmonyPatch(typeof(DummyPickupable), nameof(DummyPickupable.GetInstance))]
    class DummyPickupable_GetInstance_Patch
    {
        private static bool Prefix(DummyPickupable __instance, ref IPickupable __result)
        {
            if (__instance.PrefabId > 400U)
            {
                __result = PrefabFactory.InstantiateModdedObject(__instance.PrefabId) as IPickupable;

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Prefabs), nameof(Prefabs.LoadPrefabs))]
    class Prefabs_LoadPrefabs_Patch
    {
        private static void Postfix()
        {
            FieldInfo fi_prefabReferences = typeof(Prefabs).GetField("_prefabReferences", BindingFlags.Static | BindingFlags.NonPublic);
            List<PrefabReference> list = new List<PrefabReference>();
            list = (List<PrefabReference>)fi_prefabReferences.GetValue(null);

            foreach (KeyValuePair<string, PrefabInformation> info in Framework.PrefabInformation)
            {
                PrefabReference prefabReference = new PrefabReference();
                prefabReference.Prefab = PrefabFactory.InstantiateModdedObject(info.Key);
                prefabReference.Path = info.Key;
                prefabReference.Id = info.Value.Id;

                list = (List<PrefabReference>)fi_prefabReferences.GetValue(null);
                list.Add(prefabReference);
                fi_prefabReferences.SetValue(null, list);
            }
        }
    }

    [HarmonyPatch(typeof(MultiplayerMng), nameof(MultiplayerMng.RegisterPrefabs))]
    class MultiplayerMng_RegisterPrefabs_Patch
    {
        private static readonly AccessTools.FieldRef<MultiplayerMng, Dictionary<string, PrefabId>> _prefabsRef = AccessTools.FieldRefAccess<MultiplayerMng, Dictionary<string, PrefabId>>("_prefabs");

        private static bool Prefix(MultiplayerMng __instance)
        {
            foreach (KeyValuePair<string,PrefabInformation> info in Framework.PrefabInformation)
            {
                PrefabId prefabId = new PrefabId();
                prefabId.Value = (int)info.Value.Id;

                _prefabsRef(__instance).Add(info.Key, prefabId);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(MultiplayerMng), nameof(MultiplayerMng.TryGetPrefabName))]
    class MultiplayerMng_TryGetPrefabName_Patch
    {
        private static bool Prefix(ref bool __result, string prefabPath, out string multiplayerPrefabName)
        {
            if (prefabPath.StartsWith(Framework.MODDED_ITEM_NAME_START))
            {
                multiplayerPrefabName = prefabPath;
                __result = true;
                return false;
            }

            multiplayerPrefabName = "-";
            return true;
        }
    }*/
}