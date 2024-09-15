using System;
using HarmonyLib;
using Beam;
using System.Collections.Generic;
using Beam.Events;
using Beam.Obfuscated.Users;

namespace CustomEffects
{
    public class EffectsHarmony
    {
        [HarmonyPatch(typeof(Statistics), nameof(Statistics.ApplyStatusEffect), new Type[] { typeof(PlayerEffect), typeof(bool) })]
        class StatusEffect_CreateEffects_Patch
        {
			private static readonly AccessTools.FieldRef<Statistics, HashSet<PlayerEffect>> _playerEffectsRef = AccessTools.FieldRefAccess<Statistics, HashSet<PlayerEffect>>("_playerEffects");
			private static readonly AccessTools.FieldRef<Statistics, IPlayerSpeech> _speechRef = AccessTools.FieldRefAccess<Statistics, IPlayerSpeech>("_speech");
			private static readonly AccessTools.FieldRef<Statistics, IPlayer> _playerRef = AccessTools.FieldRefAccess<Statistics, IPlayer>("_player");

			private static bool Prefix(Statistics __instance, ref bool __result, PlayerEffect effect, bool force = false)
            {
				effect = EffectsManager.ProcessEffect(effect);

				if (__instance.Invincible && !force)
				{
					return false;
				}
				bool flag = _playerEffectsRef(__instance).Add(effect);
				if (flag)
				{
					effect.StartedTime = GameTime.Now;

					AccessTools.Method(typeof(Statistics), "OnPlayerEffectApplied", new Type[] { typeof(PlayerEffect) }).Invoke(__instance, new object[] { effect });

					if (!effect.PositiveEffect)
					{
						EventManager.RaiseEvent<AddStatsEvent>(new AddStatsEvent(_playerRef(__instance).Id, new Stats
						{
							StatusEffects = 1
						}));
					}
					bool isUnderwater = _playerRef(__instance).Movement.IsUnderwater;
					int gender = _playerRef(__instance).Gender;
					string name = effect.Name;

					if (name == "DEHYDRATION")
					{
						_speechRef(__instance).Play("STATUS_DEHYDRATION", gender, isUnderwater);
					}
					else if (name == "STARVING")
					{
						_speechRef(__instance).Play("STATUS_STARVING", gender, isUnderwater);
					}
					else if (name == "BLEEDING")
					{
						_speechRef(__instance).Play("STATUS_BLEEDING", gender, isUnderwater);
					}
					else if (name == "BROKEN_BONES")
					{
						__instance.RemoveStatusEffect(StatusEffect.SPLINT);
						_speechRef(__instance).Play("STATUS_BROKEN_BONES", gender, isUnderwater);
					}
					else if (name == "SUNBLOCK")
					{
						_speechRef(__instance).Play("STATUS_SUNBLOCK", gender, isUnderwater);
					}
					else if (name == "POISON")
					{
						_speechRef(__instance).Play("STATUS_POISON", gender, isUnderwater);
						AccessTools.Method(typeof(Statistics), "OnPoisoned").Invoke(__instance, new object[] { });
					}
					else if (name == "DIARRHOEA")
					{
						_speechRef(__instance).Play("STATUS_DIARRHOEA", gender, isUnderwater);
					}
					else if (name == "BOOST_BREATH")
					{
						_speechRef(__instance).Play("STATUS_BREATH", gender, isUnderwater);
					}
					else if (name == "MALNUTRITION")
					{
						_speechRef(__instance).Play("STATUS_MALNUTRITION", gender, isUnderwater);
					}
					else if (name == "ILLNESS")
					{
						_speechRef(__instance).Play("STATUS_ILLNESS", gender, isUnderwater);
					}
					else if (name == "TIRED")
					{
						_speechRef(__instance).Play("STATUS_TIRED", gender, isUnderwater);
					}
					else if (name == "HYPOTHERMIA")
					{
						_speechRef(__instance).Play("STATUS_HYPOTHERMIA", gender, isUnderwater);
					}
				}

				__result = flag;
				return false;
			}
		}

		[HarmonyPatch(typeof(Sunburn), nameof(Sunburn.IsDayTimeUV))]
		class Sunburn_IsDayTime_Patch
        {
			private static void Postfix(ref bool __result)
            {
				__result = Singleton<GameTime>.Instance.MilitaryTime >= 7.75f && Singleton<GameTime>.Instance.MilitaryTime <= 15.75f;
			}
        }

		[HarmonyPatch(typeof(Statistics), "SetDifficultyParams")]
		class Statistics_SetDifficultyParams_Patch
		{
			private static void Postfix()
            {
				EffectsManager.fi_caloriePerDay.SetValue(PlayerRegistry.LocalPlayer.Statistics, Main.effectsSettings.thirstAndHungerSettings.caloriesPerHour);
				EffectsManager.fi_thirstPerDay.SetValue(PlayerRegistry.LocalPlayer.Statistics, Main.effectsSettings.thirstAndHungerSettings.thirstPerHour);
			}
        }
	}
}

/* uint num = default;

if (name != null)
{
	num = 2166136261U;
	for (int i = 0; i < name.Length; i++)
	{
		num = ((uint)name[i] ^ num) * 16777619U;
	}
}

uint num = < PrivateImplementationDetails >.ComputeStringHash(name);
/*if (num <= 2583388674U)
{
	if (num <= 1123897629U)
	{
		if (num != 27142412U)
		{
			if (num != 1069152003U)
			{
				if (num == 1123897629U)
				{
					if (name == "HYPOTHERMIA")
					{
						_speechRef(__instance).Play("STATUS_HYPOTHERMIA", gender, isUnderwater);
					}
				}
			}
			else if (!(name == "HEALTH_REGENERATION"))
			{
			}
		}
		else if (name == "BROKEN_BONES")
		{
			__instance.RemoveStatusEffect(StatusEffect.SPLINT);
			_speechRef(__instance).Play("STATUS_BROKEN_BONES", gender, isUnderwater);
		}
	}
	else if (num <= 2110457223U)
	{
		if (num != 1484146070U)
		{
			if (num == 2110457223U)
			{
				if (name == "MALNUTRITION")
				{
					_speechRef(__instance).Play("STATUS_MALNUTRITION", gender, isUnderwater);
				}
			}
		}
		else if (name == "SUNBLOCK")
		{
			_speechRef(__instance).Play("STATUS_SUNBLOCK", gender, isUnderwater);
		}
	}
	else if (num != 2258548273U)
	{
		if (num == 2583388674U)
		{
			if (!(name == "SHARK_REPELLENT"))
			{
			}
		}
	}
	else if (name == "POISON")
	{
		_speechRef(__instance).Play("STATUS_POISON", gender, isUnderwater);
		AccessTools.Method(typeof(Statistics), "OnPoisoned").Invoke(__instance, new object[] {});
		//this.OnPoisoned();
	}
}
else if (num <= 3359074074U)
{
	if (num != 3324381373U)
	{
		if (num != 3335517435U)
		{
			if (num == 3359074074U)
			{
				if (name == "DIARRHOEA")
				{
					_speechRef(__instance).Play("STATUS_DIARRHOEA", gender, isUnderwater);
				}
			}
		}
		else if (name == "STARVING")
		{
			_speechRef(__instance).Play("STATUS_STARVING", gender, isUnderwater);
		}
	}
	else if (name == "BLEEDING")
	{
		_speechRef(__instance).Play("STATUS_BLEEDING", gender, isUnderwater);
	}
}
else if (num <= 4084214507U)
{
	if (num != 3891506215U)
	{
		if (num == 4084214507U)
		{
			if (name == "BOOST_BREATH")
			{
				_speechRef(__instance).Play("STATUS_BREATH", gender, isUnderwater);
			}
		}
	}
	else if (name == "ILLNESS")
	{
		_speechRef(__instance).Play("STATUS_ILLNESS", gender, isUnderwater);
	}
}
else if (num != 4150039464U)
{
	if (num == 4266079669U)
	{
		if (name == "TIRED")
		{
			_speechRef(__instance).Play("STATUS_TIRED", gender, isUnderwater);
		}
	}
}
else if (name == "DEHYDRATION")
{
	_speechRef(__instance).Play("STATUS_DEHYDRATION", gender, isUnderwater);
}*/
