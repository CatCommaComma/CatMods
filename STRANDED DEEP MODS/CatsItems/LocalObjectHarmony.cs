﻿using Beam;
using UnityEngine;
using HarmonyLib;
using Beam.Crafting;
using Funlabs;
using Beam.Utilities;
using SDPublicFramework;
using System.Collections.Generic;
using System;
using Beam.UI;
using System.Management.Instrumentation;

namespace CatsItems
{
    public class LocalObjectHarmony
    {
        /***************************  BUCKET  *****************************/


        [HarmonyPatch(typeof(Cooking), "SetCookingInShader")]
        class Cooking_SetCookingInShader_Patch
        {
            private static readonly AccessTools.FieldRef<Cooking, InteractiveObject_FOOD> _foodRef = AccessTools.FieldRefAccess<Cooking, InteractiveObject_FOOD>("_food");

            private static bool Prefix(Cooking __instance)
            {
                if (_foodRef(__instance) != null && _foodRef(__instance).gameObject != null && _foodRef(__instance).PrefabId == 405U)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Smoking), "SetSmokingInShader")]
        class Smoking_SetSmokingInShader_Patch
        {
            private static readonly AccessTools.FieldRef<Smoking, InteractiveObject_FOOD> _foodRef = AccessTools.FieldRefAccess<Smoking, InteractiveObject_FOOD>("_food");

            private static bool Prefix(Smoking __instance)
            {
                if (_foodRef(__instance) != null && _foodRef(__instance).gameObject != null && _foodRef(__instance).PrefabId == 405U)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Smoking), "OnSmoked", new Type[] { typeof(bool) })]
        class Smoking_OnSmoked_Patch
        {
            private static readonly AccessTools.FieldRef<Smoking, InteractiveObject_FOOD> _foodRef = AccessTools.FieldRefAccess<Smoking, InteractiveObject_FOOD>("_food");

            private static bool Prefix(Smoking __instance, bool loading = false)
            {
                if (_foodRef(__instance) != null && _foodRef(__instance).gameObject != null)
                {
                    if (_foodRef(__instance).PrefabId == 405U)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Cooking), "OnCooked", new Type[] { typeof(bool) })]
        class Cooking_OnCooked_Patch
        {
            private static readonly AccessTools.FieldRef<Cooking, InteractiveObject_FOOD> _foodRef = AccessTools.FieldRefAccess<Cooking, InteractiveObject_FOOD>("_food");

            private static bool Prefix(Cooking __instance, bool loading = false)
            {
                if (_foodRef(__instance) != null && _foodRef(__instance).gameObject != null)
                {
                    if (_foodRef(__instance).PrefabId == 405U)
                    {
                        (_foodRef(__instance) as Cat_Bucket).BoilToFresh();
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CampFireAudio), nameof(CampFireAudio.CampFire_StartedCooking), new Type[] { typeof(IBase), typeof(IBaseActionEventData) })]
        class CampFireAudio_CampFire_StartedCooking_Patch
        {
            private static bool Prefix(CampFireAudio __instance, IBase sender, IBaseActionEventData data)
            {
                if (sender.gameObject != null)
                {
                    Construction_CAMPFIRE currentCampfire = sender.gameObject.GetComponent<Construction_CAMPFIRE>();

                    if (currentCampfire != null && currentCampfire.Food != null && currentCampfire.Food.PrefabId == 405U)
                    {
                        (currentCampfire.Food as Cat_Bucket).StartBoiling();
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CampFireAudio), nameof(CampFireAudio.CampFire_StoppedCooking), new Type[] { typeof(IBase), typeof(IBaseActionEventData) })]
        class CampFireAudio_CampFire_StoppedCooking_Patch
        {
            private static void Postfix(CampFireAudio __instance, IBase sender, IBaseActionEventData data)
            {
                if (sender.gameObject != null)
                {
                    Construction_CAMPFIRE currentCampfire = sender.gameObject.GetComponent<Construction_CAMPFIRE>();

                    if (currentCampfire != null && currentCampfire.Food != null && currentCampfire.Food.PrefabId == 405U)
                    {
                        (currentCampfire.Food as Cat_Bucket).StopBoiling();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CampFireAudio), nameof(CampFireAudio.CampFire_Ignited), new Type[] { typeof(IBase), typeof(IBaseActionEventData) })]
        class CampFireAudio_CampFire_Ignited_Patch
        {
            private static void Postfix(CampFireAudio __instance, IBase sender, IBaseActionEventData data)
            {
                if (sender.gameObject != null)
                {
                    Construction_CAMPFIRE currentCampfire = sender.gameObject.GetComponent<Construction_CAMPFIRE>();

                    if (currentCampfire != null && currentCampfire.Food != null && currentCampfire.Food.PrefabId == 405U)
                    {
                        (currentCampfire.Food as Cat_Bucket).StartBoiling();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CampFireAudio), nameof(CampFireAudio.CampFire_Extinguished), new Type[] { typeof(IBase), typeof(IBaseActionEventData) })]
        class CampFireAudio_CampFire_Extinguished_Patch
        {
            private static void Postfix(CampFireAudio __instance, IBase sender, IBaseActionEventData data)
            {
                if (sender.gameObject != null)
                {
                    Construction_CAMPFIRE currentCampfire = sender.gameObject.GetComponent<Construction_CAMPFIRE>();

                    if (currentCampfire != null && currentCampfire.Food != null && currentCampfire.Food.PrefabId == 405U)
                    {
                        (currentCampfire.Food as Cat_Bucket).StopBoiling();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CampFireAudio), nameof(CampFireAudio.CampFire_Attached), new Type[] { typeof(IBase), typeof(IBaseActionEventData) })]
        class CampFireAudio_CampFire_Attached_Patch
        {
            private static bool Prefix(CampFireAudio __instance, IBase sender, IBaseActionEventData data)
            {
                if (sender.gameObject != null)
                {
                    Construction_CAMPFIRE currentCampfire = sender.gameObject.GetComponent<Construction_CAMPFIRE>();

                    if (currentCampfire != null && currentCampfire.Food != null && currentCampfire.Food.PrefabId == 405U)
                    {
                        AudioManager.GetAudioPlayer().Play2D(LocalEmbeddedAudio.BucketAttachSound, AudioMixerChannels.FX, AudioPlayMode.Single);
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Constructing_SPIT), nameof(Constructing_SPIT.InteractWithObject), new Type[] { typeof(IPlayer), typeof(IBase) })]
        class Constructing_SPIT_InteractWithObject_Patch
        {
            private static void Postfix(IPlayer player, IBase obj, ref bool __result)
            {
                if (obj != null && obj is Cat_Bucket && __result)
                {
                    AudioManager.GetAudioPlayer().Play2D(LocalEmbeddedAudio.BucketAttachSound, AudioMixerChannels.FX, AudioPlayMode.Single);
                }
            }
        }

        [HarmonyPatch(typeof(Farming_PLOT), nameof(Farming_PLOT.InteractWithObject), new Type[] { typeof(IPlayer), typeof(IBase) })]
        class Farming_PLOT_InteractWithObject_Patch
        {
            private static bool Prefix(IPlayer player, IBase obj)
            {
                if (obj != null && obj is Cat_Bucket)
                {
                    if ((obj as Cat_Bucket).IsSalty)
                    {
                        CatUtility.PopNotification("Cannot fill plot with salty water!", 4f);
                        return false;
                    }
                }
                return true;
            }
        }


        /******************************** LAND SHARK SIGN ***********************************/


        [HarmonyPatch(typeof(Constructing_HOOK), "CanAttach", new Type[] { typeof(IAttachable) })]
        class Constructing_HOOK_CanAttach_Patch
        {
            private static void Postfix(IAttachable attachable, ref bool __result)
            {
                if (!__result)
                {
                    __result = attachable.transform.gameObject.name.StartsWith("mod_csharksign");
                }
            }
        }


        /******************************** POCKET KNIFE ***********************************/


        [HarmonyPatch(typeof(Skinner), "Interacter_RequestGameActions", new Type[] { typeof(Interacter), typeof(IBase) })]
        class Skinner_Interacter_RequestGameActions_Patch
        {
            private static readonly AccessTools.FieldRef<Skinner, IPlayer> _playerRef = AccessTools.FieldRefAccess<Skinner, IPlayer>("_player");
            private static readonly AccessTools.FieldRef<Skinner, ISkinnable> _currentSkinnableRef = AccessTools.FieldRefAccess<Skinner, ISkinnable>("_currentSkinnable");
            private static readonly AccessTools.FieldRef<Skinner, SceneGameAction> _actionRef = AccessTools.FieldRefAccess<Skinner, SceneGameAction>("_action");

            private static bool Prefix(Skinner __instance, Interacter sender, IBase obj)
            {
                IPickupable heldObject = _playerRef(__instance).Holder.CurrentObject;

                if (!heldObject.IsNullOrDestroyed())
                {
                    if (heldObject.PrefabId == 403U)
                    {
                        if (obj.IsNullOrDestroyed() || _currentSkinnableRef(__instance).IsValid())
                        {
                            return false;
                        }

                        ISkinnable skinnable = obj.gameObject.GetComponent<BaseObject>() as ISkinnable;
                        bool isSkinned = (bool)AccessTools.Method(typeof(Skinner), "IsSkinnedByTheOtherPlayer", new Type[] { typeof(ISkinnable) }).Invoke(__instance, new object[] { skinnable });

                        if (!skinnable.IsNullOrDestroyed() && skinnable.CanSkin && !isSkinned)
                        {
                            sender.AddGameAction(_actionRef(__instance));
                        }
                        return false;
                    }
                }
                return true;
            }
        }

        //for skinning animation
        [HarmonyPatch(typeof(EventItemProxies), nameof(EventItemProxies.GetProxy), new Type[] {typeof(CraftingType)})]
        class EventItemProxies_GetProxy_Patch
        {
            private static GameObject _pocketKnifeProxy = null;

            private static void Postfix(EventItemProxies __instance, ref GameObject __result, CraftingType type)
            {
                if (type.InteractiveType == InteractiveType.TOOLS_KNIFE && PlayerRegistry.LocalPlayer.Holder.CurrentObject != null && PlayerRegistry.LocalPlayer.Holder.CurrentObject.PrefabId == 403U)
                {
                    if (_pocketKnifeProxy == null)
                    {
                        BaseObject proxy = PrefabFactory.InstantiateModdedObject(403U, true);
                        proxy.GetComponent<Rigidbody>().isKinematic = true;
                        proxy.transform.SetParent(__result.transform.parent, false);
                        proxy.transform.position = __result.transform.position;
                        proxy.transform.localRotation = __result.transform.localRotation;

                        //fine tune
                        proxy.transform.Rotate(new Vector3(0,180,0),Space.Self);//sides -towards //to sides (-away, +towards player rightish)    //up+ down-
                        proxy.transform.localPosition = new Vector3(proxy.transform.localPosition.x, proxy.transform.localPosition.y - 0.03f, proxy.transform.localPosition.z + 0.13f);

                        _pocketKnifeProxy = proxy.gameObject;
                    }

                    __result = _pocketKnifeProxy;
                }
            }
        }

        //for hiding knife blade
        [HarmonyPatch(typeof(InteractiveObject), nameof(InteractiveObject.SetLOD), new Type[] { typeof(int) })]
        class InteractiveObject_SetLOD_Patch
        {
            private static void Postfix(InteractiveObject __instance, int level = -1)
            {
                if (__instance.PrefabId == 403U)
                {
                    Transform knifeBlade = __instance.gameObject.transform.Find("Knife_Blade");

                    if (level == -1)
                    {
                        knifeBlade.localScale = new Vector3(0, 0, 0);
                    }
                    else if (level == 0)
                    {
                        knifeBlade.localScale = new Vector3(1, 1, 1);
                    }
                }
            }
        }


        /******************************** OPEN CANNED BEANS ***********************************/


        [HarmonyPatch(typeof(InteractiveObject), nameof(InteractiveObject.Destroy))]
        class InteractiveObject_Destroy_Patch
        {
            private static bool Prefix(InteractiveObject __instance)
            {
                if (__instance.PrefabId == 408U)
                {
                    BaseObject cannedBeans = PrefabFactory.InstantiateModdedObject(411U);

                    if (cannedBeans != null)
                    {
                        cannedBeans.transform.position = __instance.transform.position;
                        cannedBeans.transform.rotation = __instance.transform.rotation;

                        InteractiveObject interactiveObject = cannedBeans as InteractiveObject;

                        MultiplayerMng.Destroy(__instance, DestroyDelay.SAFE);

                        interactiveObject.ReferenceId = new MiniGuid();
                        cannedBeans.gameObject.SetActive(true);

                        AudioManager.GetAudioPlayer().Play2D(BasicSounds.BreakSound, AudioMixerChannels.Voice, AudioPlayMode.Single);

                        interactiveObject.enabled = true;
                        interactiveObject.EnablePhysics();
                    }

                    return false;
                }
                return true;
            }
        }


        /*************************************** EXTRA MACHETE DAMAGE TO CREATURES AND PLANTS ***************************************/


        [HarmonyPatch(typeof(BaseUtilities), nameof(BaseUtilities.DoPlayerDamageMultipliers_Hunting))]
        class BaseUtilities_DoPlayerDamageMultipliers_Hunting_Patch
        {
            private static void Postfix(ref float damage, IPickupable damager)
            {
                //machette ids
                if (damager.PrefabId == 410U || damager.PrefabId == 346U)
                {
                    damage *= 2.15f;
                }
            }
        }

        [HarmonyPatch(typeof(BaseObject), nameof(BaseObject.Damage), new Type[] { typeof(float), typeof(IBase) })]
        class BaseObjects_Damage_Patch
        {
            private static bool Prefix(ref float damage, IBase damager, BaseObject __instance)
            {
                IPickupable pickupable = damager as IPickupable;
                if (!pickupable.IsNullOrDestroyed())
                {
                    if ((pickupable.PrefabId == 410U || pickupable.PrefabId == 346U) && IsPlant(__instance.PrefabId))
                    {
                        damage *= 5f;
                    }
                }

                return true;
            }

            private static bool IsPlant(uint id)
            {
                switch (id)
                {
                    case 180U: //big palm
                        return true;

                    case 181U: //small palm
                        return true;

                    case 30U: //yucca
                        return true;

                    case 207U: //pine big
                        return true;

                    case 206U: //pine small
                        return true;

                    case 49U: //ficus triple
                        return true;

                    case 47U: //ficus small
                        return true;
                }

                return false;
            }
        }


        /************************************** NO MORE INFINITE DRINKABLE COCONUTS ***********************************/


        [HarmonyPatch(typeof(PileSlotStorage), nameof(PileSlotStorage.CanPush), new Type[] { typeof(IPickupable), typeof(bool) })]
        class PileSlotStorage_CanPush_Patch
        {
            private static bool Prefix(ref bool __result, IPickupable obj, bool notification = true)
            {
                if (obj.IsNullOrDestroyed() || !obj.CraftingType.IsPileType())
                {
                    return true;
                }
                else if (obj.gameObject.name.StartsWith("COCONUT_DRINKABLE") && obj.gameObject.GetComponent<InteractiveObject_FOOD>().Servings == 0)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }


        /******************************************* LIGHTER STUFF ************************************************/


        [HarmonyPatch(typeof(Construction_CAMPFIRE), nameof(Construction_CAMPFIRE.InteractWithObject), new Type[] { typeof(IPlayer), typeof(IBase) })]
        class Construction_CAMPFIRE_InteractWithObject_Patch
        {
            private static void Postfix(Construction_CAMPFIRE __instance, IPlayer player, IBase obj)
            {
                if (!obj.IsNullOrDestroyed() && __instance.IsBurning && obj.gameObject.name.StartsWith("mod_clighter"))
                {
                    InteractiveObject lighter = obj.gameObject.GetComponent<InteractiveObject>();

                    if (lighter.DurabilityPoints > 0.01f)
                    {
                        lighter.DurabilityPoints -= 3f;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Interactive_FIRE_TORCH), nameof(Interactive_FIRE_TORCH.InteractWithObject), new Type[] { typeof(IPlayer), typeof(IBase) })]
        class Interactive_FIRE_TORCH_InteractWithObject_Patch
        {
            private static bool Prefix(Interactive_FIRE_TORCH __instance, ref bool __result, IPlayer player, IBase obj)
            {
                if (!obj.IsNullOrDestroyed() && obj.gameObject.name.StartsWith("mod_clighter"))
                {
                    InteractiveObject lighter = obj.gameObject.GetComponent<InteractiveObject>();

                    if (lighter != null && lighter.DurabilityPoints > 0.01f && !__instance.IsBurning)
                    {
                        lighter.DurabilityPoints -= 1.5f;
                        __instance.Ignite();
                        __result = true;
                    }
                    else
                    {
                        AccessTools.Method(typeof(Interactive_FIRE_TORCH), "Extinguish").Invoke(__instance, null);
                        __result = false;
                    }

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Interactive_FIRE_TORCH), nameof(Interactive_FIRE_TORCH.GetInteractionDescription), new Type[] { typeof(int), typeof(IBase) })]
        class Interactive_FIRE_TORCH_GetInteractionDescription_Patch
        {
            private static bool Prefix(Interactive_FIRE_TORCH __instance, ref string __result, int playerId, IBase obj)
            {
                if (!obj.IsNullOrDestroyed())
                {
                    IFlammable flammable = obj as IFlammable;
                    if (flammable != null && !__instance.IsBurning && flammable.IsBurning)
                    {
                        __result = "ITEM_INTERACTION_DESCRIPTION_LIGHT";
                        return false;
                    }
                }
                return true;
            }
        }


        /******************************************* SHARK FIN ************************************************/

        //spawn at shark
        [HarmonyPatch(typeof(IDisintegratableMixins), nameof(IDisintegratableMixins.Disintegrate), new Type[] { typeof(IDisintegratable) })]
        class IDisintegratableMixins_Disintegrate_Patch
        {
            private static Vector3 _location = default;

            private static bool Prefix(IDisintegratable disintegratable)
            {
                if (disintegratable.gameObject.name.ToLower().StartsWith("shark"))
                {
                    _location = disintegratable.transform.position;
                }
                return true;
            }

            private static void Postfix(IDisintegratable disintegratable)
            {
                if (_location != default)
                {
                    BaseObject sharkFin = PrefabFactory.InstantiateModdedObject(417U, true);
                    if (sharkFin != null)
                    {
                        sharkFin.ReferenceId = new MiniGuid();

                        _location.y += 1.6f;
                        sharkFin.transform.position = _location;
                        sharkFin.transform.rotation = default;

                        _location = default;
                    }
                }
            }

            //so that the fin doesn't look like pure coal when smoked
            [HarmonyPatch(typeof(Smoking), "SetSmokingInShader")]
            class Smoking_SetSmokingInShader_Patch
            {
                private static readonly AccessTools.FieldRef<Smoking, float> _smokingHoursRef = AccessTools.FieldRefAccess<Smoking, float>("_smokingHours");
                private static readonly AccessTools.FieldRef<Smoking, float> _originalSmokingHoursRef = AccessTools.FieldRefAccess<Smoking, float>("_originalSmokingHours");

                private static bool Prefix(Smoking __instance)
                {
                    if (__instance.Food.PrefabId == 417)
                    {
                        float value = __instance.IsSmoked ? 1f : Mathf.Clamp01(1f - _smokingHoursRef(__instance) / _originalSmokingHoursRef(__instance));
                        for (int i = 0; i < __instance.Food.renderers.Length; i++)
                        {
                            __instance.Food.renderers[i].GetPropertyBlock(__instance.Food.PropertyBlock);
                            __instance.Food.PropertyBlock.SetFloat("_Smoked", value * 0.7f);
                            __instance.Food.renderers[i].SetPropertyBlock(__instance.Food.PropertyBlock);
                        }

                        return false;
                    }
                    return true;
                }
            }


            /******************************************* FLARE ************************************************/


            [HarmonyPatch(typeof(FlareGunProjectile), "Update")]
            class FlareGunProjectile_Update_Patch
            {
                private static readonly AccessTools.FieldRef<FlareGunProjectile, GameObject> _smokeTrailRef = AccessTools.FieldRefAccess<FlareGunProjectile, GameObject>("_smokeTrail");
                private static readonly AccessTools.FieldRef<FlareGunProjectile, bool> _hoveringRef = AccessTools.FieldRefAccess<FlareGunProjectile, bool>("_hovering");
                private static readonly AccessTools.FieldRef<FlareGunProjectile, float> _lifeTimeRef = AccessTools.FieldRefAccess<FlareGunProjectile, float>("_lifeTime");

                private static bool Prefix(FlareGunProjectile __instance)
                {
                    if (__instance.gameObject.name == "moddedflare")
                    {
                        _lifeTimeRef(__instance) += Time.deltaTime;

                        //because .GetComponent each frame is bad
                        if (!_flares.ContainsKey(__instance)) _flares.Add(__instance, __instance.transform.parent.gameObject.GetComponent<Cat_Flare>());
                        Cat_Flare flare = _flares[__instance];

                        __instance.transform.position = flare.Lid.position;

                        if (flare.Drowning)
                        {
                            if (flare.Initial)
                            {
                                flare.FadeOutSound(SECONDS_TO_EXTINGUISH_DROWN);
                                _smokeTrailRef(__instance).GetComponent<ParticleSystem>().Stop();
                                flare.Initial = true;
                            }

                            float newIntensity = ((SECONDS_TO_EXTINGUISH_DROWN - (_lifeTimeRef(__instance) - flare.DrownAt)) / SECONDS_TO_EXTINGUISH_DROWN) * flare.InitialIntensity;

                            if (newIntensity <= 0)
                            {
                                if (_lifeTimeRef(__instance) - flare.DrownAt >= SECONDS_TO_DESTROY_DROWN)
                                {
                                    flare.CleanUp(__instance);
                                    _flares.Remove(__instance);
                                }
                                return false;
                            }

                            flare.CurrentLight.intensity = newIntensity;
                            return false;
                        }

                        if (!_hoveringRef(__instance))
                        {
                            __instance.gameObject.GetComponent<Rigidbody>().isKinematic = true;

                            flare.CurrentLight = __instance.gameObject.GetComponentInChildren<Light>();
                            flare.InitialIntensity = flare.CurrentLight.intensity;

                            _hoveringRef(__instance) = true;
                        }

                        if (__instance.transform.position.y < 0.01f && !flare.Drowning)
                        {
                            flare.InitialIntensity = flare.CurrentLight.intensity;
                            flare.DrownAt = _lifeTimeRef(__instance);
                            flare.Drowning = true;
                        }
                        else if (_lifeTimeRef(__instance) >= SECONDS_LIFETIME)
                        {
                            if (flare.Initial)
                            {
                                _smokeTrailRef(__instance).GetComponent<ParticleSystem>().Stop();
                                flare.FadeOutSound(SECONDS_TO_DESTROY);
                                flare.Initial = true;
                            }

                            float newIntensity = ((SECONDS_TO_DESTROY - (_lifeTimeRef(__instance) - SECONDS_LIFETIME)) / SECONDS_TO_DESTROY) * flare.InitialIntensity;
                            flare.CurrentLight.intensity = newIntensity;

                            if (newIntensity <= 0)
                            {
                                flare.CleanUp(__instance);
                                _flares.Remove(__instance);
                            }
                        }
                    }
                    return true;
                }

                private static Dictionary<FlareGunProjectile, Cat_Flare> _flares = new Dictionary<FlareGunProjectile, Cat_Flare>();
                private const float SECONDS_TO_EXTINGUISH_DROWN = 1.5f;
                private const float SECONDS_TO_DESTROY_DROWN = 5f;
                private const float SECONDS_LIFETIME = 200f;
                private const float SECONDS_TO_DESTROY = 5f;
            }


            /******************************************* WHISTLE SCARE SEAGULL ************************************************/


            [HarmonyPatch(typeof(FlockChild), "Idle_Update")]
            class FlockChild_Idle_Update_Patch
            {
                private static void Postfix(FlockChild __instance)
                {
                    if (Cat_Whistle.IsAnyWhistled(out IPlayer player) && __instance.CurrentState.Equals(FlockChildState.Idle))
                    {
                        __instance.Scare(player.transform.position);
                    }
                }
            }


            /******************************************* GOGGLES BETTER VISIBILITY UNDERWATER ************************************************/


            [HarmonyPatch(typeof(CetoSettings), nameof(CetoSettings.UpdateWaterVisibility))]
            class CetoSettings_UpdateWaterColor_Patch
            {
                private static readonly AccessTools.FieldRef<CetoSettings, float> _belowVisibilityRef = AccessTools.FieldRefAccess<CetoSettings, float>("_belowVisibility");

                private static void Postfix(CetoSettings __instance, float normalizedTime)
                {
                    if (Cat_GoggleLight.IsAnyEquipped || Cat_Goggles.IsAnyEquipped)
                    {
                        if (_extraVisibility < MAX_EXTRA_VISIBILITY) _extraVisibility += Time.deltaTime * 10f;
                    }
                    else if (_extraVisibility > 0f) _extraVisibility -= Time.deltaTime * 10f;

                    Mathf.Clamp(_extraVisibility, 0, 10);
                    _belowVisibilityRef(__instance) += _extraVisibility;
                }

                private static float _extraVisibility = 0f;
                private const float MAX_EXTRA_VISIBILITY = 11f;
            }


            /******************************************* SHARK BAIT ICON CHANGE ************************************************/


            [HarmonyPatch(typeof(InventoryData), nameof(InventoryData.GetIcon))]
            class InventoryData_GetIcon_Patch
            {
                private static void Postfix(ref Sprite __result, InventoryData __instance)
                {
                    Cat_SharkBait bait = __instance.Value as Cat_SharkBait;

                    if (bait != null && bait.Matured && Main.SpoiledSharkBaitSprite != null)
                    {
                        __result = Main.SpoiledSharkBaitSprite;
                    }
                }
            }


            /******************************************* MITCHELL ************************************************/

            //horrible but unfair otherwise
            [HarmonyPatch(typeof(ObjectPoolManager), "InternalCreate", new Type[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) })]
            class ObjectPoolManager_InternalCreate_Patch
            {
                private static bool Prefix(GameObject prefab, Vector3 position, Quaternion rotation, ref GameObject __result)
                {
                    if (prefab.name.Contains("Starfish_Poisonous"))
                    {
                        foreach (Vector3 vector3 in UrchinDecayer.BannedRespawnPositions)
                        {
                            if (vector3.Equals(position))
                            {
                                __result = new GameObject("Destroyed_Starfish");
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }


            //************** TESTING ***************/


            /*[HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] {typeof(GameObject)})]
            class Object_Instantiate1_Patch
            {
                private static void Postfix(ref UnityEngine.Object __result)
                {
                    Debug.Log($"One: {__result.name}");
                }
            }

            [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(GameObject), typeof(Transform) })]
            class Object_Instantiate4_Patch
            {
                private static void Postfix(ref UnityEngine.Object __result)
                {
                    Debug.Log($"Two: {__result.name}");
                }
            }

            [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion) })]
            class Object_Instantiate2_Patch
            {
                private static void Postfix(ref UnityEngine.Object __result)
                {
                    Debug.Log($"Three: {__result.name}");
                    if (__result.name.Contains("Starfish_Poisonous")) Debug.Log(Environment.StackTrace);
                }
            }

            [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Instantiate), new Type[] { typeof(GameObject), typeof(Vector3), typeof(Quaternion), typeof(Transform) })]
            class Object_Instantiate3_Patch
            {
                private static void Postfix(ref UnityEngine.Object __result)
                {
                    Debug.Log($"Four: {__result.name}");
                }
            }*/
        }
    }
}