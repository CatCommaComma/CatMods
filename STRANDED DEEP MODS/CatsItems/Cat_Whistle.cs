using Beam;
using UnityEngine;
using Beam.UI;
using System.Collections;

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

        public override bool ValidatePrimary(IBase obj)
        {
            return false;
        }

        private void Update()
        {
            if (IsPickedUp && !MainMenuPresenter.Instance.IsGamePaused && !_isWhistlingStatic)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    _isWhistlingStatic = true;
                    _whistler = Owner;
                    _whistler.Character.Animater.Play(Primary, null, 1f, 1f, false, global::AnimationBlendMode.None);

                    StartCoroutine(Whistle());
                }
            }

            if (MainMenuPresenter.Instance.IsGamePaused && _whistleSoundHandle != null)
            {
                AudioManager.GetAudioPlayer().Stop(_whistleSoundHandle);
                _isWhistlingStatic = false;
                _whistler = null;
            }
        }

        private IEnumerator Whistle()
        {
            _whistleSoundHandle = AudioManager.GetAudioPlayer().Play3D(_whistleSound.clip, transform, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
            IAudioSource audioSource = AudioManager.GetAudioPlayer().GetAudioSource(_whistleSoundHandle);
            float timeElapsed = 0;

            while (1 > timeElapsed)
            {
                timeElapsed += Time.deltaTime * 8;
                audioSource.Volume = Mathf.Lerp(0, 1, timeElapsed);

                yield return null;
            }

            while (Input.GetKey(KeyCode.Mouse0))
            {
                if (MainMenuPresenter.Instance.IsGamePaused) audioSource.Volume = 0;
                else audioSource.Volume = 1;

                yield return null;
            }

            timeElapsed = 0;
            while (1 > timeElapsed)
            {
                timeElapsed += Time.deltaTime * 8;
                audioSource.Volume = Mathf.Lerp(1, 0, timeElapsed);
                yield return null;
            }

            AudioManager.GetAudioPlayer().Stop(_whistleSoundHandle);
            _whistler.Character.Animater.Play(Idle, null, 1f, 1f, false, global::AnimationBlendMode.None);

            _isWhistlingStatic = false;
            _whistler = null;
        }

        private AudioActorHandle _whistleSoundHandle;
        private AudioSource _whistleSound;

        private static bool _isWhistlingStatic = false;
        private static IPlayer _whistler = null;
    }
}