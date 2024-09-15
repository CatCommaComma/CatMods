using System;
using System.Reflection;
using Beam;
using UnityEngine;
using HarmonyLib;

namespace FoodRestoresHealth
{
    public class DrunkHarmony
    {
        [HarmonyPatch(typeof(InteractiveObject), nameof(InteractiveObject.ValidatePrimary), new Type[] { typeof(IBase)})]
        public class InteractiveObject_ValidatePrimary_Postfix
        {
            private static void Postfix(IBase obj, InteractiveObject __instance, ref bool __result)
            {
                InteractiveObject_FUELCAN fuelCan = __instance.gameObject.GetComponent<InteractiveObject_FUELCAN>();

                if (fuelCan != null)
                {
                    if (DrunkManager.CanDrink && (Mathf.Approximately(fuelCan.Fuel, 0.2f) || fuelCan.Fuel > 0.19f) && !PlayerRegistry.LocalPlayer.Movement.IsUnderwater)
                    {
                        IPlayer player = (IPlayer)(typeof(InteractiveObject).GetField("_owner", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance));

                        if (!player.Statistics.CanEat)
                        {
                            __result = false;
                            return;
                        }

                        if (__instance.Primary == AnimationType.NONE)
                        {
                            typeof(InteractiveObject).GetField("_primary", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, (AnimationType)13);
                        }

                        DrunkManager.UpdateDrunkStats(player, 1);

                        if (player.Gender == 0) AudioManager.GetAudioPlayer().Play2D(Main.drinkAlch, AudioMixerChannels.Voice, AudioPlayMode.Single);
                        else AudioManager.GetAudioPlayer().Play2D(Main.drinkAlchFem, AudioMixerChannels.Voice, AudioPlayMode.Single);

                        Main.fi_FuelAmount.SetValue(fuelCan, (fuelCan.Fuel - 0.2f));
                        DrunkManager.CanDrink = false;
                    }
                    else __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Statistics), nameof(Statistics.RemoveStatusEffect), new Type[] { typeof(PlayerEffect) })]
        public class Statistics_RemoveStatusEffect_Postfix
        {
            private static void Postfix(PlayerEffect effect, Statistics __instance)
            {
                if ((effect == Main.relaxedEffect || effect == Main.intoxicatedEffect || effect == Main.wastedEffect || effect == Main.overdoseEffect) && DrunkManager.DrunkState != DrunkState.NONE && !DrunkManager.drunkardIsBusy)
                {
                    DrunkManager.ResetDrunk(__instance, true, true);
                }
            }
        }

        [HarmonyPatch(typeof(GameTime), nameof(GameTime.Load), new Type[] { typeof(Beam.Serialization.Json.JObject) })]
        class GameTime_Load_Patch
        {
            private static void Postfix(GameTime __instance)
            {
                DrunkManager.InitializeDrunk();
            }
        }

        [HarmonyPatch(typeof(BaseUtilities), "DoHuntingMultiplier", new Type[] { typeof(float), typeof(IPlayer) })]
        class BaseUtilities_DoHuntingMultiplier_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (DrunkManager.DrunkState == DrunkState.OVERDOSED)
                {
                    __result *= 2f;
                }
                else if (DrunkManager.DrunkState == DrunkState.WASTED)
                {
                    __result *= 1.4f;
                }
            }
        }

        [HarmonyPatch(typeof(BaseUtilities), "DoHarvestingMultiplier", new Type[] { typeof(float), typeof(IPlayer) })]
        class BaseUtilities_DoHarvestingMultiplier_Patch
        {
            private static void Postfix(ref float __result)
            {
                if (DrunkManager.DrunkState == DrunkState.OVERDOSED)
                {
                    __result *= 2.5f;
                }
                else if (DrunkManager.DrunkState == DrunkState.WASTED)
                {
                    __result *= 1.5f;
                }
            }
        }


        [HarmonyPatch(typeof(GameCalendar), "GameTime_DayTick")]
        class GameCalendar_GameTime_DayTick_Patch
        {
            private static void Postfix()
            {
                Debug.Log("Daytick");

                if (DrunkManager.DrankToday) DrunkManager.DrankToday = false;
                else
                {
                    DrunkManager._tolerance -= 40;
                }
            }
        }

        [HarmonyPatch(typeof(Beam.Crafting.Crafter), nameof(Beam.Crafting.Crafter.Craft), new Type[] { typeof(Beam.Crafting.CraftingCombination), typeof(Funlabs.MiniGuid) })]
        class Crafter_Craft_Patch
        {
            private static bool Prefix(Beam.Crafting.CraftingCombination combination, Funlabs.MiniGuid referenceId, Beam.Crafting.Crafter __instance)
            {
                if (DrunkManager.DrunkState == DrunkState.OVERDOSED)
                {
                    int random = UnityEngine.Random.Range(0, 26);

                    if (random == 0) return true;

                    DrunkManager.RandomFailCraftSpeech();

                    return false;
    
                }
                else if (DrunkManager.DrunkState == DrunkState.WASTED)
                {
                    int random = UnityEngine.Random.Range(0, 21);

                    if (random <= 4) return true;

                    DrunkManager.RandomFailCraftSpeech();

                    return false;
                }

                return true;
            }
        }
    }
}
