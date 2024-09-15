using Beam;
using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using StrandedDeepWetAndColdMod;

namespace FoodRestoresHealth
{
    public class DrunkManager : MonoBehaviour
    {
        public static DrunkManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    CreateNewInstance();
                }

                return _instance;
            }
        }

        public static bool DrankToday { get { return _drankToday; } set { _drankToday = value; } }
        public static DrunkState DrunkState { get { return _drunkState; } }
        public static bool CanDrink { get { return _canDrink; } set { _canDrink = value; } }

        public static void CreateNewInstance()
        {
            GameObject go = Instantiate(new GameObject("DrunkManager"));
            _instance = go.AddComponent<DrunkManager>();
        }

        internal static void InitializeDrunk()
        {
            CreateNewInstance();

            if (Game.State == GameState.NEW_GAME)
            {
                _drunkLevel = 0;
                _randomFactor = 0;
                _tolerance = UnityEngine.Random.Range(-350, 350);

                UpdateDrunkStats(PlayerRegistry.LocalPlayer, 0);

                return;
            }

            switch (Options.GeneralSettings.LastSaveSlotUsed)
            {
                case 0:
                    _drunkLevel = Main.saveData.drunkLevelOne;
                    _tolerance = Main.saveData.toleranceOne;
                    break;

                case 1:
                    _drunkLevel = Main.saveData.drunkLevelTwo;
                    _tolerance = Main.saveData.toleranceTwo;
                    break;

                case 2:
                    _drunkLevel = Main.saveData.drunkLevelThree;
                    _tolerance = Main.saveData.toleranceThree;
                    break;

                case 3:
                    _drunkLevel = Main.saveData.drunkLevelFour;
                    _tolerance = Main.saveData.toleranceFour;
                    break;
            }
            _randomFactor = Main.saveData.randomFactor;

            UpdateDrunkStats(PlayerRegistry.LocalPlayer, 0);
        }

        public static void HandleDrunkard()
        {
            if (_drunkState >= DrunkState.WASTED)
            {
                _eventTimer += Time.deltaTime;

                if (EVENT_TRIGGER < _eventTimer)
                {
                    DoEvent();

                    _eventTimer = 0;
                }

                if (!blinking && !noBlinking) Instance.StartCoroutine(BlinkScreen());
            }
            else if (blinking)
            {
                StopBlinking();
            }

            if (!_canDrink)
            {
                _drinkTimer += Time.deltaTime;

                if (_drinkTimer > DRINK_COOLDOWN)
                {
                    _canDrink = true;
                    _drinkTimer = 0f;
                }
            }

            if (Main.settings.usingWetMod) DoWetMod(currentPlayer);
        }

        private static void StopBlinking()
        {
            ClearBlinkingEffect();
            Instance.StopCoroutine(BlinkScreen());
            blinking = false;

            _eventTimer = 0;
        }

        private static bool blinking = false;

        private static IEnumerator BlinkScreen()
        {
            blinking = true;

            while (true)
            {
                if (_drunkState == DrunkState.WASTED)
                {
                    yield return new WaitForSecondsRealtime(3f);

                    Main.blackblink.CrossFadeAlpha(1f, 3.5f, true);

                    yield return new WaitForSecondsRealtime(6.5f);

                    Main.blackblink.CrossFadeAlpha(1f, 3.5f, true);

                    yield return new WaitForSecondsRealtime(6.5f);

                    Main.blackblink.CrossFadeAlpha(0.425f, 2.5f, true);

                    yield return new WaitForSecondsRealtime(12.5f);
                }
                else if (_drunkState == DrunkState.OVERDOSED)
                {
                    yield return new WaitForSecondsRealtime(3f);

                    Main.blackblink.CrossFadeAlpha(1f, 2.5f, true);

                    yield return new WaitForSecondsRealtime(5.5f);

                    Main.blackblink.CrossFadeAlpha(0.55f, 4f, true);

                    yield return new WaitForSecondsRealtime(8f);

                    Main.whiteblink.CrossFadeAlpha(1f, 5f, true);

                    yield return new WaitForSecondsRealtime(6.5f);

                    Main.whiteblink.CrossFadeAlpha(0f, 5f, true);
                }
                else
                {
                    ClearBlinkingEffect();
                    blinking = false;
                    yield break;
                }

                yield return null;
            }
        }

        private static void ClearBlinkingEffect()
        {
            _eventTimer = -4f;
            Main.blackblink.CrossFadeAlpha(0f, 4f, true);
            Main.whiteblink.CrossFadeAlpha(0f, 4f, true);
        }

        private static int CheckStomach(Statistics player)
        {
            int num = 1;

            if (player.CaloriesPercent <= 0.45f) num++;
            if (player.CaloriesPercent <= 0.15f) num++;
            if (player.HasStatusEffect(Main.hangoverEffect) || player.HasStatusEffect(Main.heavyhangoverEffect) || player.HasStatusEffect(Main.overdoseRecoveryEffect)) num += 3;

            return num;
        }

        private static int ToleranceEffect()
        {
            if (_tolerance >= 2250) return 2;
            else if (_tolerance >= 750) return 1;
            else if (_tolerance >= -25) return 0;
            else if (_tolerance >= -550) return -1;
            else return -2;
        }

        public static void ResetDrunk(Statistics player, bool withHangover = true, bool natural = false)
        {
            _drunkLevel = 0;
            _randomFactor = 0;
            _eventTimer = 0;

            switch (_drunkState)
            {
                case DrunkState.RELAXED:
                    if (!natural) player.RemoveStatusEffect(Main.relaxedEffect);
                    break;

                case DrunkState.INTOXICATED:
                    if (!natural) player.RemoveStatusEffect(Main.intoxicatedEffect);
                    if (withHangover) player.ApplyStatusEffect(Main.hangoverEffect);
                    break;

                case DrunkState.WASTED:
                    if (!natural) player.RemoveStatusEffect(Main.wastedEffect);
                    ApplyDrunkBuffs(_drunkState, currentPlayer, true);
                    if (withHangover) player.ApplyStatusEffect(Main.heavyhangoverEffect);
                    break;

                case DrunkState.OVERDOSED:
                    if (!natural) player.RemoveStatusEffect(Main.overdoseEffect);
                    ApplyDrunkBuffs(_drunkState, currentPlayer, true);
                    if (withHangover) player.ApplyStatusEffect(Main.overdoseRecoveryEffect);
                    break;
            }

            StopBlinking();
            UpdateTunnelVision(0, 15f);

            _drunkState = DrunkState.NONE;
        }

        public static void ApplyDrunk(Statistics player, bool removeOtherDrunkEffects = true)
        {
            switch (_drunkState)
            {
                case DrunkState.RELAXED:
                    player.ApplyStatusEffect(Main.relaxedEffect);
                    break;

                case DrunkState.INTOXICATED:
                    player.ApplyStatusEffect(Main.intoxicatedEffect);
                    break;

                case DrunkState.WASTED:
                    player.ApplyStatusEffect(Main.wastedEffect);
                    break;

                case DrunkState.OVERDOSED:
                    player.ApplyStatusEffect(Main.overdoseEffect);
                    break;
            }

            if (removeOtherDrunkEffects)
            {
                if (_drunkState != DrunkState.RELAXED) player.RemoveStatusEffect(Main.relaxedEffect);
                if (_drunkState != DrunkState.INTOXICATED) player.RemoveStatusEffect(Main.intoxicatedEffect);
                if (_drunkState != DrunkState.WASTED) player.RemoveStatusEffect(Main.wastedEffect);
                if (_drunkState != DrunkState.OVERDOSED) player.RemoveStatusEffect(Main.overdoseEffect);
            }
        }

        public static void ApplyDrunkBuffs(DrunkState type, IPlayer player, bool removeAll = false) //sitas
        {
            if (removeAll)
            {
                if (type == DrunkState.OVERDOSED)
                {
                    Main.fi_maxStamina.SetValue(player.Statistics, 100f);
                }

                return;
            }

            switch (type)
            {
                case DrunkState.OVERDOSED:

                    Main.fi_maxStamina.SetValue(player.Statistics, 200f);
                    break;
            }
        }

        public static void UpdateDrunkStats(IPlayer player, int value = 0)
        {
            drunkardIsBusy = true;
            _drankToday = true;

            Statistics statistics = player.Statistics;
            currentPlayer = player;

            _eventTimer = 0f;
            float vomitChance = 0f;

            if (_drunkLevel <= 0) _randomFactor = UnityEngine.Random.Range(-1, 2);

            int multiplier = CheckStomach(statistics);
            _drunkLevel += (value * multiplier);
            UpdateTunnelVision(_drunkLevel, 2f);

            int toleranceEffect = ToleranceEffect();
            int drunkError = toleranceEffect + _randomFactor;

            if (_drunkLevel <= RELAXEDMAX + drunkError)
            {
                _drunkState = DrunkState.RELAXED;
                if (value != 0) _tolerance += TOLERANCE_GAIN;
            }
            else if (_drunkLevel <= INTOXICATEDMAX + drunkError)
            {
                if (_drunkState != DrunkState.INTOXICATED) ApplyDrunkBuffs(DrunkState.INTOXICATED, player);
                _drunkState = DrunkState.INTOXICATED;
                if (value != 0) _tolerance += TOLERANCE_GAIN * 2;
            }
            else if (_drunkLevel <= WASTEDMAX + drunkError)
            {
                if (_drunkState != DrunkState.WASTED)
                {
                    ApplyDrunkBuffs(DrunkState.WASTED, player);
                    RandomSpeech();
                }
                _drunkState = DrunkState.WASTED;
                if (value != 0) _tolerance += TOLERANCE_GAIN;
                vomitChance = 20f;
            }
            else
            {
                ClearBlinkingEffect();

                if (_drunkState != DrunkState.OVERDOSED)
                {
                    ApplyDrunkBuffs(_drunkState, player, true);
                    _drunkState = DrunkState.OVERDOSED;
                    ApplyDrunkBuffs(_drunkState, player, false);
                }

                if (value != 0) _tolerance += TOLERANCE_GAIN * -1;
                vomitChance = 50f;
            }

            if (value != 0)
            {
                ApplyDrunk(statistics, true);
                statistics.Eat(InteractiveType.TOOLS_JERRYCAN, MeatProvenance.Other, 14, 0, 0, 0, 4f, vomitChance, false);
            }
            drunkardIsBusy = false;
        }

        private static void UpdateTunnelVision(int level, float duration = 1.75f, bool reset = false)
        {
            if (level > 10) level = 10;
            Main.tunnelVision.CrossFadeAlpha(((float)level * 0.1f), duration, true);
        }

        internal static void DoEvent()
        {
            IPlayer player = PlayerRegistry.LocalPlayer;

            bool isMale = player.Gender == 0;
            bool canHallucinateVisually = (_drunkState == DrunkState.OVERDOSED);
            bool isWasted = _drunkState == DrunkState.WASTED;

            int num = UnityEngine.Random.Range(0, 99);

            if (num <= 7)
            {
                if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.mansigh, AudioMixerChannels.Voice, AudioPlayMode.Single);
                else AudioManager.GetAudioPlayer().Play2D(Main.femsigh, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 8 && num <= 15)
            {
                if (!isWasted)
                {
                    if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.manlaugh1, AudioMixerChannels.Voice, AudioPlayMode.Single);
                    else AudioManager.GetAudioPlayer().Play2D(Main.femlaugh1, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
                else
                {
                    if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.manbreathe, AudioMixerChannels.Voice, AudioPlayMode.Single);
                    else AudioManager.GetAudioPlayer().Play2D(Main.fembreathe, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
            }
            else if (num >= 16 && num <= 23)
            {
                if (!isWasted)
                {
                    if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.manlaugh2, AudioMixerChannels.Voice, AudioPlayMode.Single);
                    else AudioManager.GetAudioPlayer().Play2D(Main.femlaugh2, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
                else
                {
                    if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.mancough1, AudioMixerChannels.Voice, AudioPlayMode.Single);
                    else AudioManager.GetAudioPlayer().Play2D(Main.femcough1, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
            }
            else if (num >= 24 && num <= 31)
            {
                if (!isWasted)
                {
                    if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.manbelch1, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
                else
                {
                    if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.mancough2, AudioMixerChannels.Voice, AudioPlayMode.Single);
                    else AudioManager.GetAudioPlayer().Play2D(Main.femcough2, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
            }
            else if (num >= 32 && num <= 39)
            {
                if (!isWasted)
                {
                    AudioManager.GetAudioPlayer().Play2D(Main.manbelch2, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
                else
                {
                    if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.mancough2, AudioMixerChannels.Voice, AudioPlayMode.Single);
                    else AudioManager.GetAudioPlayer().Play2D(Main.femcough2, AudioMixerChannels.Voice, AudioPlayMode.Single);
                }
            }
            else if (num >= 40 && num <= 51)
            {
                if (!player.Statistics.CanEat) return;
                player.Statistics.Eat(InteractiveType.TOOLS_JERRYCAN, MeatProvenance.Other, -50, 0, 0, 0, 0f, -100f, false);
            }
            else if (num >= 52 && num <= 59)
            {
                AudioManager.GetAudioPlayer().Play2D(Main.manhic1, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 60 && num <= 66)
            {
                if (isMale) AudioManager.GetAudioPlayer().Play2D(Main.mancough3, AudioMixerChannels.Voice, AudioPlayMode.Single);
                else AudioManager.GetAudioPlayer().Play2D(Main.femcough3, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 86 && num <= 99 && canHallucinateVisually)
            {
                PlayRandomHallucination();
            }
        }

        internal static void PlayRandomHallucination()
        {
            int num = UnityEngine.Random.Range(0, 104);

            if (num <= 12)
            {
                AudioManager.GetAudioPlayer().Play2D(Main.beepmult, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 13 && num <= 24)
            {
                AudioManager.GetAudioPlayer().Play2D(Main.beepsingle, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 25 && num <= 36)
            {
                AudioManager.GetAudioPlayer().Play2D(Main.splashL, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 37 && num <= 48)
            {
                AudioManager.GetAudioPlayer().Play2D(Main.splashR, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 49 && num <= 60)
            {
                AudioManager.GetAudioPlayer().Play2D(Main.boarR2, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 61 && num <= 72)
            {
                AudioManager.GetAudioPlayer().Play2D(Main.boarL2, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else if (num >= 73 && num <= 81)
            {
                Instance.StartCoroutine(EyesTrip());
            }
            else if (num >= 82 && num <= 92)
            {
                Instance.StartCoroutine(SexyTrip());
            }
            else if (num >= 93 && num <= 103)
            {
                Instance.StartCoroutine(LSDTrip());
            }
        }

        private static bool noBlinking = false;

        private static IEnumerator EyesTrip()
        {
            noBlinking = true;
            StopBlinking();

            AudioManager.GetAudioPlayer().Play2D(Main.beepmult, AudioMixerChannels.Voice, AudioPlayMode.Single);

            yield return new WaitForSecondsRealtime(2.5f);

            AudioManager.GetAudioPlayer().Play2D(Main.halluc4sound, AudioMixerChannels.Voice, AudioPlayMode.Single);

            Main.blackblink.CrossFadeAlpha(1f, 0.1f, true);
            Main.halluc4.CrossFadeAlpha(1f, 0.1f, true);
            Main.tunnelVision.CrossFadeAlpha(1f, 0.1f, true);

            yield return new WaitForSecondsRealtime(0.1f);

            int time = UnityEngine.Random.Range(1, 7);

            Singleton<GameTime>.Instance.MilitaryTime = time;

            yield return new WaitForSecondsRealtime(0.15f);

            ApplyRandomHallucinationEffect(currentPlayer.Statistics);

            Main.blackblink.CrossFadeAlpha(0.7f, 0.2f, true);
            Main.halluc4.CrossFadeAlpha(0.2f, 0.2f, true);
            Main.tunnelVision.CrossFadeAlpha(0f, 6f, true);

            yield return new WaitForSecondsRealtime(0.4f);

            Main.blackblink.CrossFadeAlpha(0.95f, 0.25f, true);
            Main.halluc4.CrossFadeAlpha(0.5f, 0.25f, true);

            yield return new WaitForSecondsRealtime(0.35f);

            Main.blackblink.CrossFadeAlpha(0f, 5f, true);
            Main.halluc4.CrossFadeAlpha(0f, 3f, true);

            yield return new WaitForSecondsRealtime(5f);

            ResetDrunk(PlayerRegistry.LocalPlayer.Statistics, true);

            noBlinking = false;
        }

        private static IEnumerator SexyTrip()
        {
            noBlinking = true;
            StopBlinking();

            yield return new WaitForSecondsRealtime(2.5f);

            AudioManager.GetAudioPlayer().Play2D(Main.halluc5sound, AudioMixerChannels.Voice, AudioPlayMode.Single);

            Main.halluc5.CrossFadeAlpha(1f, 12.5f, true);

            yield return new WaitForSecondsRealtime(23f);

            Main.halluc5.CrossFadeAlpha(0f, 8.5f, true);

            yield return new WaitForSecondsRealtime(8.5f);

            ApplyRandomHallucinationEffect(PlayerRegistry.LocalPlayer.Statistics);
            noBlinking = false;
        }

        private static IEnumerator LSDTrip()
        {
            AudioManager.GetAudioPlayer().Play2D(Main.halluc3sound, AudioMixerChannels.Voice, AudioPlayMode.Single);
            Main.halluc3.CrossFadeAlpha(1f, 7f, true);

            yield return new WaitForSecondsRealtime(7f);

            Main.halluc3.CrossFadeAlpha(0.7f, 2f, true);

            yield return new WaitForSecondsRealtime(1f);

            Main.halluc3.CrossFadeAlpha(0.9f, 4f, true);

            yield return new WaitForSecondsRealtime(6f);

            Main.halluc3.CrossFadeAlpha(0.5f, 2f, true);

            yield return new WaitForSecondsRealtime(1f);

            Main.halluc3.CrossFadeAlpha(0.9f, 5f, true);

            yield return new WaitForSecondsRealtime(7f);

            Main.halluc3.CrossFadeAlpha(0f, 7f, true);

            yield return new WaitForSecondsRealtime(6.5f);

            ApplyRandomHallucinationEffect(PlayerRegistry.LocalPlayer.Statistics);
        }
        
        private static void ApplyRandomHallucinationEffect(Statistics player)
        {
            int random = UnityEngine.Random.Range(1, 6);

            switch (random)
            {
                case 1:
                    player.ApplyStatusEffect(Main.confusedhallucinationEffect);
                    break;
                case 2:
                    player.ApplyStatusEffect(Main.losthallucinationEffect);
                    break;
                case 3:
                    player.ApplyStatusEffect(Main.scaredhallucinationEffect);
                    break;
                case 4:
                    player.ApplyStatusEffect(Main.terrifiedhallucinationEffect);
                    break;
                case 5:
                    player.ApplyStatusEffect(Main.panichallucinationEffect);
                    break;
            }
        }

        private static void DoWetMod(IPlayer p1)
        {
            if (currentPlayer == null) return;

            if (_sleepDeductorStorage > 0)
            {
                float currentSleep = (float)Main.fi_sleepAmount.GetValue(p1.Statistics);
                if (currentSleep - 0.013f > 0f) currentSleep -= 0.0135f;
                Main.fi_sleepAmount.SetValue(p1.Statistics, currentSleep);
                _sleepDeductorStorage--;
            }

            if (_temperatureRaiserStorage > 0)
            {
                if (StrandedDeepWetAndColdMod.Main.bodytemperatureEffect.CurrentTemperature <= (float)ParameterValues.HOT_THRESHOLD) StrandedDeepWetAndColdMod.Main.bodytemperatureEffect.CurrentTemperature += 0.0175f;
                _temperatureRaiserStorage--;
            }
        }

        private static void RandomSpeech()
        {
            int num = UnityEngine.Random.Range(1, 14);
            string text = "Ainn't yuu sexyy, gas cannn...";

            switch (num)
            {
                case 2:
                    text = "Lovely dyeyh todhay...";
                    break;

                case 3:
                    text = ".. yuurr drunk...";
                    break;

                case 4:
                    text = "Being.. beinn..bng...?...";
                    break;

                case 5:
                    text = "This iis BOMMB!!!";
                    break;

                case 6:
                    text = "woahg..";
                    break;

                case 7:
                    text = "...pambambampamm pam bam...pambpambampam pam PAM!!...";
                    break;

                case 8:
                    text = "Nnvvgnah givvv yuuu upp... nvggggnlch yuuu daunhf...";
                    break;

                case 9:
                    text = "Better not THRAUWH ahp...";
                    break;

                case 10:
                    text = "I'm. Not. Drunkk.";
                    break;

                case 11:
                    text = "...Wmpfgghhf.";
                    break;

                case 12:
                    text = "Won morr...!!!";
                    break;

                case 13:
                    text = "Jusrobh cannt compeat.";
                    break;
            }

            DoSpeech(text);
        }

        public static void RandomFailCraftSpeech()
        {
            int num = UnityEngine.Random.Range(1, 11);
            string text = "Naghhh... not feeein like it..";

            switch (num)
            {
                case 2:
                    text = "thas WAYY over my.... ead right neow";
                    break;

                case 3:
                    text = ".. yuuo want wHAT?";
                    break;

                case 4:
                    text = "yeahhg.. onn it..";
                    break;

                case 5:
                    text = "..id rathr go for a drink...";
                    break;

                case 6:
                    text = "nuh uh...";
                    break;

                case 7:
                    text = "no";
                    break;

                case 8:
                    text = ".....";
                    break;

                case 9:
                    text = "tink i jus spild me brew all oer de bluprint...";
                    break;

                case 10:
                    text = "nee mo beer.... fo braing powr";
                    break;
            }

            DoSpeech(text);
        }

        public static void DoSpeech(string text = "Unspecified text")
        {
            PlayerSpeech ps = PlayerRegistry.LocalPlayer.PlayerSpeech;
            ps.View.SubtitlesLabel.Text = text;
            ps.View.Show();
            Task.Delay(3350).ContinueWith(delegate (Task t) { ps.View.Hide(); });
        }

        private static DrunkManager _instance;
        private static bool _canDrink;
        private static bool _drankToday = false;

        internal static float _eventTimer = 0f;
        private static float _drinkTimer = 0f;

        internal static int _drunkLevel = 0;
        internal static int _randomFactor = 0;
        internal static int _tolerance = 0;

        private const int TOLERANCE_GAIN = 10;
        internal const int EVENT_TRIGGER = 40;
        private const float DRINK_COOLDOWN = 2.5f;

        private const int RELAXEDMAX = 3;
        private const int INTOXICATEDMAX = 8;
        private const int WASTEDMAX = 14;

        private static float _sleepDeductorStorage = 0f;
        private static float _temperatureRaiserStorage = 0f;

        public static bool drunkardIsBusy = false;

        public static IPlayer currentPlayer;

        private static DrunkState _drunkState = DrunkState.NONE;
    }

    public enum DrunkState
    {
        NONE,
        RELAXED,
        INTOXICATED,
        WASTED,
        OVERDOSED
    }
}
