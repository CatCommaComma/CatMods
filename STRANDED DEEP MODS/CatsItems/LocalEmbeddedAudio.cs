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
        public static AudioClip BucketAttachSound { get { return _soundBucketAttach; } }

        public static void Initialize()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            _soundPills = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modPills.wav", thisAssembly));
            _soundMorphine = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modMorphine.wav", thisAssembly));
            _soundDrinkBottle = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modDrink.wav", thisAssembly));
            _soundLighterOpen = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modLighterOpen.wav", thisAssembly));
            _soundBucketAttach = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("CatsItems.audio.modBucketAttach.wav", thisAssembly));
        }

        private static AudioClip _soundPills;
        private static AudioClip _soundMorphine;
        private static AudioClip _soundDrinkBottle;
        private static AudioClip _soundLighterOpen;
        private static AudioClip _soundBucketAttach;
    }
}