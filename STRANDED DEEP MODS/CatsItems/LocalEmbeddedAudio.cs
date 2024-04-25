using UnityEngine;
using SDPublicFramework;
using System.Reflection;

namespace CatsItems
{
    public class LocalEmbeddedAudio
    {
        public static AudioClip PopPillsSound { get { return _soundPills; } }
        public static AudioClip MorphineInjectionSound { get { return _soundMorphine; } }
        public static AudioClip DrinkWaterBottle { get { return _soundDrinkBottle; } }
        public static AudioClip OpenLighterSound { get { return _soundLighterOpen; } }
        public static AudioClip LighterStrikeSound { get { return _lighterStrikeSound; } }
        public static AudioClip BucketAttachSound { get { return _soundBucketAttach; } }
        public static AudioClip FillLighter { get { return _soundFillLighter; } }
        public static AudioClip SlimyHit { get { return _slimyHit; } }

        public static void Initialize()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            _soundPills = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modPills.wav", thisAssembly));
            _soundMorphine = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modMorphine.wav", thisAssembly));
            _soundDrinkBottle = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modDrink.wav", thisAssembly));
            _soundLighterOpen = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modLighterOpen.wav", thisAssembly));
            _soundBucketAttach = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modBucketAttach.wav", thisAssembly));
            _lighterStrikeSound = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.lighterstrike.wav", thisAssembly));
            _soundFillLighter = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.filllighter.wav", thisAssembly));
            _slimyHit = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.poisonoushit.wav", thisAssembly));
        }

        private static AudioClip _soundPills;
        private static AudioClip _soundMorphine;
        private static AudioClip _soundDrinkBottle;
        private static AudioClip _soundLighterOpen;
        private static AudioClip _soundBucketAttach;
        private static AudioClip _lighterStrikeSound;
        private static AudioClip _soundFillLighter;
        private static AudioClip _slimyHit;
    }
}