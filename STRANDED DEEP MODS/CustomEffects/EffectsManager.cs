using System.Collections.Generic;
using Beam;
using System.Reflection;
using UnityEngine;

namespace CustomEffects
{
    internal class EffectsManager
    {
        public static void ApplySettings()
        {
            UpdateListValues();
        }

        internal static void InitializeEffectInformation()
        {
            Main.statusEffects = new List<EffectInformation>();

            EffectInformation effectInformation = new EffectInformation("HEALTH_REGENERATION", 0f, 0f, 100f, -1f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("BLEEDING", 0f, 0f, 9.7f, -1f); //name, fluids/hour, calories/hour, health/hour, duration(hour)
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("POISON", 0f, 0f, 9.7f, 72f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("BROKEN_BONES", 0f, 0f, 0f, -1f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("STARVING", 0f, 0f, -14.5f, -1f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("DEHYDRATION", 0f, 0f, -14.5f, -1f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("DIARRHOEA", 0f, 0f, -700f, 1f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("BOOST_BREATH", 0f, 0f, 0f, 3f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("SHARK_REPELLENT", 0f, 0f, 0f, 12f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("SUNBLOCK", 0f, 0f, 0f, 12f);
            Main.statusEffects.Add(effectInformation);

            effectInformation = new EffectInformation("SPLINT", 0f, 0f, 0f, 48f);
            Main.statusEffects.Add(effectInformation);
        }

        public static void EnableNormalPreset()
        {
            Main.effectsSettings.healthySettings.healthEffect = 100f;

            Main.effectsSettings.bleedSettings.healthEffect = -9.7f;
            Main.effectsSettings.bleedSettings.caloriesEffect = 0f;
            Main.effectsSettings.bleedSettings.fluidsEffect = 0f;
            Main.effectsSettings.bleedSettings.duration = -1f;

            Main.effectsSettings.poisonSettings.healthEffect = -9.7f;
            Main.effectsSettings.poisonSettings.caloriesEffect = 0f;
            Main.effectsSettings.poisonSettings.fluidsEffect = 0f;
            Main.effectsSettings.poisonSettings.duration = 72f;

            Main.effectsSettings.brokenlegSettings.healthEffect = 0f;
            Main.effectsSettings.brokenlegSettings.caloriesEffect = 0f;
            Main.effectsSettings.brokenlegSettings.fluidsEffect = 0f;
            Main.effectsSettings.brokenlegSettings.duration = -1f;

            Main.effectsSettings.diarrheaSettings.healthEffect = 0f;
            Main.effectsSettings.diarrheaSettings.caloriesEffect = 0f;
            Main.effectsSettings.diarrheaSettings.fluidsEffect = -700f;
            Main.effectsSettings.diarrheaSettings.duration = 1f;

            Main.effectsSettings.starvationSettings.healthEffect = -14.5f;
            Main.effectsSettings.dehydrationSettings.healthEffect = -14.5f;

            Main.effectsSettings.splintSettings.duration = 48f;

            Main.effectsSettings.boostbreathSettings.healthEffect = 0f;
            Main.effectsSettings.boostbreathSettings.caloriesEffect = 0f;
            Main.effectsSettings.boostbreathSettings.fluidsEffect = 0f;
            Main.effectsSettings.boostbreathSettings.duration = 3f;

            Main.effectsSettings.sharkrepellentSettings.healthEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.caloriesEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.fluidsEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.duration = 12f;

            Main.effectsSettings.sunblockSettings.healthEffect = 0f;
            Main.effectsSettings.sunblockSettings.caloriesEffect = 0f;
            Main.effectsSettings.sunblockSettings.fluidsEffect = 0f;
            Main.effectsSettings.sunblockSettings.duration = 3f;

            Main.effectsSettings.thirstAndHungerSettings.thirstPerHour = 700f;
            Main.effectsSettings.thirstAndHungerSettings.caloriesPerHour = 350f;

            Main.effectsSettings.sunburnEffects = true;
            Main.effectsSettings.brokenlegEffects = false;
            Main.effectsSettings.runningDisallowed = false;

            normalPreset = false;
        }

        public static void EnableHardPreset()
        {
            Main.effectsSettings.healthySettings.healthEffect = 100f;

            Main.effectsSettings.bleedSettings.healthEffect = -19.4f;
            Main.effectsSettings.bleedSettings.caloriesEffect = 0f;
            Main.effectsSettings.bleedSettings.fluidsEffect = 0f;
            Main.effectsSettings.bleedSettings.duration = -1f;

            Main.effectsSettings.poisonSettings.healthEffect = -19.4f;
            Main.effectsSettings.poisonSettings.caloriesEffect = 0f;
            Main.effectsSettings.poisonSettings.fluidsEffect = 0f;
            Main.effectsSettings.poisonSettings.duration = 36f;

            Main.effectsSettings.brokenlegSettings.healthEffect = 0f;
            Main.effectsSettings.brokenlegSettings.caloriesEffect = 0f;
            Main.effectsSettings.brokenlegSettings.fluidsEffect = 0f;
            Main.effectsSettings.brokenlegSettings.duration = -1f;

            Main.effectsSettings.diarrheaSettings.healthEffect = 0f;
            Main.effectsSettings.diarrheaSettings.caloriesEffect = 0f;
            Main.effectsSettings.diarrheaSettings.fluidsEffect = -700f;
            Main.effectsSettings.diarrheaSettings.duration = 1f;

            Main.effectsSettings.starvationSettings.healthEffect = -29f;
            Main.effectsSettings.dehydrationSettings.healthEffect = -29f;

            Main.effectsSettings.splintSettings.duration = 48f;

            Main.effectsSettings.boostbreathSettings.healthEffect = 0f;
            Main.effectsSettings.boostbreathSettings.caloriesEffect = 0f;
            Main.effectsSettings.boostbreathSettings.fluidsEffect = 0f;
            Main.effectsSettings.boostbreathSettings.duration = 3f;

            Main.effectsSettings.sharkrepellentSettings.healthEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.caloriesEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.fluidsEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.duration = 12f;

            Main.effectsSettings.sunblockSettings.healthEffect = 0f;
            Main.effectsSettings.sunblockSettings.caloriesEffect = 0f;
            Main.effectsSettings.sunblockSettings.fluidsEffect = 0f;
            Main.effectsSettings.sunblockSettings.duration = 3f;

            Main.effectsSettings.thirstAndHungerSettings.thirstPerHour = 1400f;
            Main.effectsSettings.thirstAndHungerSettings.caloriesPerHour = 700f;

            Main.effectsSettings.sunburnEffects = true;
            Main.effectsSettings.brokenlegEffects = true;
            Main.effectsSettings.runningDisallowed = false;

            hardPreset = false;
        }

        public static void EnableRealisticPreset()
        {
            Main.effectsSettings.healthySettings.healthEffect = 7.3f;

            Main.effectsSettings.bleedSettings.healthEffect = -150f;
            Main.effectsSettings.bleedSettings.caloriesEffect = -55f;
            Main.effectsSettings.bleedSettings.fluidsEffect = -100f;
            Main.effectsSettings.bleedSettings.duration = -1f;

            Main.effectsSettings.poisonSettings.healthEffect = -14.5f;
            Main.effectsSettings.poisonSettings.caloriesEffect = -25f;
            Main.effectsSettings.poisonSettings.fluidsEffect = -35f;
            Main.effectsSettings.poisonSettings.duration = -1f;

            Main.effectsSettings.brokenlegSettings.healthEffect = -2f;
            Main.effectsSettings.brokenlegSettings.caloriesEffect = -15f;
            Main.effectsSettings.brokenlegSettings.fluidsEffect = -15f;
            Main.effectsSettings.brokenlegSettings.duration = -1f;

            Main.effectsSettings.diarrheaSettings.healthEffect = 0f;
            Main.effectsSettings.diarrheaSettings.caloriesEffect = -30f;
            Main.effectsSettings.diarrheaSettings.fluidsEffect = -200f;
            Main.effectsSettings.diarrheaSettings.duration = 5f;

            Main.effectsSettings.starvationSettings.healthEffect = -9.7f;
            Main.effectsSettings.dehydrationSettings.healthEffect = -19.4f;

            Main.effectsSettings.splintSettings.duration = 96f;

            Main.effectsSettings.boostbreathSettings.healthEffect = 0f;
            Main.effectsSettings.boostbreathSettings.caloriesEffect = 0f;
            Main.effectsSettings.boostbreathSettings.fluidsEffect = 0f;
            Main.effectsSettings.boostbreathSettings.duration = 4f;

            Main.effectsSettings.sharkrepellentSettings.healthEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.caloriesEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.fluidsEffect = 0f;
            Main.effectsSettings.sharkrepellentSettings.duration = 8f;

            Main.effectsSettings.sunblockSettings.healthEffect = 0f;
            Main.effectsSettings.sunblockSettings.caloriesEffect = 0f;
            Main.effectsSettings.sunblockSettings.fluidsEffect = 0f;
            Main.effectsSettings.sunblockSettings.duration = 8f;

            Main.effectsSettings.thirstAndHungerSettings.thirstPerHour = 900f;
            Main.effectsSettings.thirstAndHungerSettings.caloriesPerHour = 300f;

            Main.effectsSettings.sunburnEffects = true;
            Main.effectsSettings.brokenlegEffects = true;
            Main.effectsSettings.runningDisallowed = true;

            realisticPreset = false;
        }

        private static void UpdateListValues()
        {
            foreach (EffectInformation einfo in Main.statusEffects)
            {
                if (einfo != null)
                {
                    if (einfo.Name == "HEALTH_REGENERATION")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.healthySettings.healthEffect;
                    }
                    else if (einfo.Name == "SUNBLOCK")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.sunblockSettings.healthEffect;
                        einfo.FluidsPerHour = Main.effectsSettings.sunblockSettings.fluidsEffect;
                        einfo.CaloriesPerHour = Main.effectsSettings.sunblockSettings.caloriesEffect;
                        einfo.Duration = Main.effectsSettings.sunblockSettings.duration;
                    }
                    else if (einfo.Name == "SHARK_REPELLENT")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.sharkrepellentSettings.healthEffect;
                        einfo.FluidsPerHour = Main.effectsSettings.sharkrepellentSettings.fluidsEffect;
                        einfo.CaloriesPerHour = Main.effectsSettings.sharkrepellentSettings.caloriesEffect;
                        einfo.Duration = Main.effectsSettings.sharkrepellentSettings.duration;
                    }
                    else if (einfo.Name == "BOOST_BREATH")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.boostbreathSettings.healthEffect;
                        einfo.FluidsPerHour = Main.effectsSettings.boostbreathSettings.fluidsEffect;
                        einfo.CaloriesPerHour = Main.effectsSettings.boostbreathSettings.caloriesEffect;
                        einfo.Duration = Main.effectsSettings.boostbreathSettings.duration;
                    }
                    else if (einfo.Name == "SPLINT")
                    {
                        einfo.Duration = Main.effectsSettings.splintSettings.duration;
                    }
                    else if (einfo.Name == "STARVING")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.starvationSettings.healthEffect;
                    }
                    else if (einfo.Name == "DEHYDRATION")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.dehydrationSettings.healthEffect;
                    }
                    else if (einfo.Name == "BLEEDING")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.bleedSettings.healthEffect;
                        einfo.FluidsPerHour = Main.effectsSettings.bleedSettings.fluidsEffect;
                        einfo.CaloriesPerHour = Main.effectsSettings.bleedSettings.caloriesEffect;
                        einfo.Duration = Main.effectsSettings.bleedSettings.duration;
                    }
                    else if (einfo.Name == "BROKEN_BONES")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.brokenlegSettings.healthEffect;
                        einfo.FluidsPerHour = Main.effectsSettings.brokenlegSettings.fluidsEffect;
                        einfo.CaloriesPerHour = Main.effectsSettings.brokenlegSettings.caloriesEffect;
                        einfo.Duration = Main.effectsSettings.brokenlegSettings.duration;
                    }
                    else if (einfo.Name == "POISON")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.poisonSettings.healthEffect;
                        einfo.FluidsPerHour = Main.effectsSettings.poisonSettings.fluidsEffect;
                        einfo.CaloriesPerHour = Main.effectsSettings.poisonSettings.caloriesEffect;
                        einfo.Duration = Main.effectsSettings.poisonSettings.duration;
                    }
                    else if (einfo.Name == "DIARRHOEA")
                    {
                        einfo.HealthPerHour = Main.effectsSettings.diarrheaSettings.healthEffect;
                        einfo.FluidsPerHour = Main.effectsSettings.diarrheaSettings.fluidsEffect;
                        einfo.CaloriesPerHour = Main.effectsSettings.diarrheaSettings.caloriesEffect;
                        einfo.Duration = Main.effectsSettings.diarrheaSettings.duration;
                    }
                }
            }

            if (PlayerRegistry.LocalPlayer != null)
            {
                fi_caloriePerDay.SetValue(PlayerRegistry.LocalPlayer.Statistics, Main.effectsSettings.thirstAndHungerSettings.caloriesPerHour);
                fi_thirstPerDay.SetValue(PlayerRegistry.LocalPlayer.Statistics, Main.effectsSettings.thirstAndHungerSettings.thirstPerHour);
            }
        }

        public static PlayerEffect ProcessEffect(PlayerEffect effect)
        {
            foreach (EffectInformation einfo in Main.statusEffects)
            {
                if (einfo != null)
                {
                    if (einfo.Name == effect.Name)
                    {
                        effect = new PlayerEffect(effect.Name, effect.DisplayName, effect.PositiveEffect, einfo.FluidsPerHour, einfo.CaloriesPerHour, effect.TemperaturePerHour, einfo.HealthPerHour, einfo.Duration);

                        break;
                    }
                }
            }

            return effect;
        }

        public static void UpdateNewEffects()
        {
            if (PlayerRegistry.LocalPlayer != null)
            {
                Statistics player = PlayerRegistry.LocalPlayer.Statistics;

                if (Main.effectsSettings.sunburnEffects) DoSunburn(player);
                if (Main.effectsSettings.brokenlegEffects)  CheckSplint(player);
            }

            ManageUV();
        }

        private static void ManageUV()
        {
            if (Game.State == GameState.MAIN_MENU && loadedUV)
            {
                playerUV = 0;
                loadedUV = false;
            }

            if (Game.State == GameState.LOAD_GAME && !loadedUV)
            {
                if (Options.GeneralSettings.LastSaveSlotUsed == 0) playerUV = Main.effectsSettings.UVFirst;
                else if (Options.GeneralSettings.LastSaveSlotUsed == 1) playerUV = Main.effectsSettings.UVSecond;
                else if (Options.GeneralSettings.LastSaveSlotUsed == 2) playerUV = Main.effectsSettings.UVThird;
                else if (Options.GeneralSettings.LastSaveSlotUsed == 3) playerUV = Main.effectsSettings.UVFourth;

                loadedUV = true;
            }
            else if (Game.State != GameState.NEW_GAME && !loadedUV)
            {
                playerUV = 0;
                loadedUV = true;
            }
        }

        private static void DoSunburn(Statistics player)
        {
            bool hasSunblock = player.HasStatusEffect(StatusEffect.SUNBLOCK);
            bool hasSunstroke = player.HasStatusEffect(StatusEffect.SUNSTROKE);

            UpdateUV(hasSunblock, hasSunstroke);

            if (hasSunblock || !hasSunstroke) return;

            bool hasLightSunburn = player.HasStatusEffect(Main.sunburnLightEffect);
            bool hasHeavySunburn = player.HasStatusEffect(Main.sunburnHeavyEffect);

            if (playerUV <= 0)
            {
                player.RemoveStatusEffect(Main.sunburnHeavyEffect);
            }

            if (playerUV > 30240f && hasLightSunburn) //30240 = 3.5min x time scale x 4 (?)
            {
                player.ApplyStatusEffect(Main.sunburnHeavyEffect);
                player.RemoveStatusEffect(Main.sunburnLightEffect);
                return;
            }

            if (!hasHeavySunburn)
            {
                player.ApplyStatusEffect(Main.sunburnLightEffect);
            }
        }

        private static void UpdateUV(bool hasSunblock, bool hasSunstroke)
        {
            if (hasSunstroke && playerUV < 60480f)
            {
                playerUV += (Time.deltaTime * 4f * GameTime.TIME_SCALE);
                return;
            }
            else if (playerUV > 0)
            {
                playerUV -= (Time.deltaTime * GameTime.TIME_SCALE * (hasSunblock ? 1f : 2.5f));
            }
        }

        private static void CheckSplint(Statistics player)
        {
            bool hasSplint = player.HasStatusEffect(StatusEffect.SPLINT);
            bool hasBrokenleg = player.HasStatusEffect(StatusEffect.BROKEN_BONES);

            if (!hasSplint && !hasBrokenleg) return;

            bool hasRecentBrokenleg = player.HasStatusEffect(Main.recentBrokenLegEffect);
            bool hasRecentBrokenlegExtra = player.HasStatusEffect(Main.recentBrokenLegExtraEffect);

            if ((hasRecentBrokenleg || hasRecentBrokenlegExtra) && Main.effectsSettings.runningDisallowed) NoRunningSplint(true);

            if (hasBrokenleg && hasRecentBrokenleg)
            {
                player.RemoveStatusEffect(Main.recentBrokenLegEffect);
                player.ApplyStatusEffect(Main.recentBrokenLegExtraEffect);
                return;
            }

            if (hasSplint && !hasRecentBrokenlegExtra)
            {
                player.ApplyStatusEffect(Main.recentBrokenLegEffect);
            }
        }

        private static void NoRunningSplint(bool constrict)
        {
            Movement sr = PlayerRegistry.LocalPlayer.Movement;

            fi_sprintButtonDown.SetValue(sr, !constrict);
        }

        public static void EffectDebugger()
        {
            if (PlayerRegistry.LocalPlayer != null)
            {
                Statistics player = PlayerRegistry.LocalPlayer.Statistics;

                if (Main.effectsSettings.effectsDebugger.removeEffects)
                {
                    List<PlayerEffect> effects = GetPlayerEffectsList(player);

                    foreach (PlayerEffect effect in effects)
                    {
                        player.RemoveStatusEffect(effect);
                    }

                    Main.effectsSettings.effectsDebugger.removeEffects = false;

                    return;
                }

                switch ((int)Main.effectsSettings.effectsDebugger.givenEffect)
                {
                    case 0:
                        break;

                    case 1:
                        player.ApplyStatusEffect(StatusEffect.BLEEDING);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 2:
                        player.ApplyStatusEffect(StatusEffect.POISON);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 3:
                        player.ApplyStatusEffect(StatusEffect.BROKEN_BONES);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 4:
                        player.ApplyStatusEffect(StatusEffect.DIARRHOEA);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 5:
                        player.ApplyStatusEffect(StatusEffect.SPLINT);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 6:
                        player.ApplyStatusEffect(StatusEffect.BOOST_BREATH);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 7:
                        player.ApplyStatusEffect(StatusEffect.SHARK_REPELLENT);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 8:
                        player.ApplyStatusEffect(StatusEffect.SUNBLOCK);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 9:
                        player.ApplyStatusEffect(Main.sunburnLightEffect);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 10:
                        player.ApplyStatusEffect(Main.sunburnHeavyEffect);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 11:
                        player.ApplyStatusEffect(Main.recentBrokenLegEffect);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;

                    case 12:
                        player.ApplyStatusEffect(Main.recentBrokenLegExtraEffect);
                        Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
                        break;
                }
            }

            Main.effectsSettings.effectsDebugger.givenEffect = EffectsDebugger.GiveEffects.None;
            Main.effectsSettings.effectsDebugger.removeEffects = false;
        }

        public static List<PlayerEffect> GetPlayerEffectsList(Statistics player)
        {
            HashSet<PlayerEffect> effectsHash = (HashSet<PlayerEffect>)player.PlayerEffects;

            List<PlayerEffect> effects = new List<PlayerEffect>();

            foreach (PlayerEffect effect in effectsHash)
            {
                effects.Add(effect);
            }

            effectsHash.Clear();

            return effects;
        }

        public static float playerUV = 0f;
        private static bool loadedUV = false;

        public static FieldInfo fi_sprintButtonDown;
        public static FieldInfo fi_caloriePerDay;
        public static FieldInfo fi_thirstPerDay;

        public static bool normalPreset = false;
        public static bool hardPreset = false;
        public static bool realisticPreset = false;

        public static bool updateInfo = false;
    }
}
