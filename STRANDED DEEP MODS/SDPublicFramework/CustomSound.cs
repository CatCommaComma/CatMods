using UnityEngine;

namespace SDPublicFramework
{
    public class CustomSound : IConsumableSound
    {
        public AudioClip ConsumableClip { get; set; }
        public CustomSound(AudioClip consumableClip)
        {
            ConsumableClip = consumableClip;
        }
    }
}