using Beam;
using UnityEngine;
using Beam.UI;

namespace CatsItems
{
    public class Cat_Whistle : InteractiveObject, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical
    {
        public static bool IsAnyWhistled(out IPlayer player)
        {
            player = _whistler;
            return _isWhistlingStatic;
        }

        protected override void Awake()
        {
            base.Awake();
            _whistleSound = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (IsPickedUp && !MainMenuPresenter.Instance.IsGamePaused)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    //Debug.Log("--- whistle start ---");
                    _whistleSoundActor = AudioManager.GetAudioPlayer().Play3D(_whistleSound.clip, transform, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
                    _isWhistlingStatic = true;
                    _whistler = Owner;
                }
                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    //Debug.Log("--- whistle stop ---");
                    AudioManager.GetAudioPlayer().Stop(_whistleSoundActor);
                    //play sound fall off here
                    _isWhistlingStatic = false;
                    _whistler = null;
                }
            }

            if (MainMenuPresenter.Instance.IsGamePaused && _whistleSoundActor != null)
            {
                AudioManager.GetAudioPlayer().Stop(_whistleSoundActor);
                _isWhistlingStatic = false;
                _whistler = null;
            }
        }

        private AudioActorHandle _whistleSoundActor;
        private AudioSource _whistleSound;

        private static bool _isWhistlingStatic = false;
        private static IPlayer _whistler = null;
    }
}