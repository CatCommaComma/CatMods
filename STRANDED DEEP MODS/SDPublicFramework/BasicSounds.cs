using UnityEngine;
using System.Reflection;

namespace SDPublicFramework
{
    public class BasicSounds
    {
        public static AudioClip DrinkSound { get { return _soundDrink; } }
        public static AudioClip EatSound { get { return _soundEat; } }
        public static AudioClip BreakSound { get { return _soundBreak; } }
        public static AudioClip EquipSound { get { return _soundEquip; } }
        public static AudioClip UnequipSound { get { return _soundUnequip; } }

        public static void Initialize()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            _soundDrink = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("SDPublicFramework.audio.modDrink.wav", thisAssembly));
            _soundEat = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("SDPublicFramework.audio.modEat.wav", thisAssembly));
            _soundBreak = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("SDPublicFramework.audio.modBreak.wav", thisAssembly));
            _soundEquip = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("SDPublicFramework.audio.modEquip.wav", thisAssembly));
            _soundUnequip = WavUtility.ToAudioClip(CatUtility.ExtractEmbeddedResource("SDPublicFramework.audio.modUnequip.wav", thisAssembly));
        }

        private static AudioClip _soundDrink;
        private static AudioClip _soundEat;
        private static AudioClip _soundBreak;
        private static AudioClip _soundEquip;
        private static AudioClip _soundUnequip;
    }
}
//a