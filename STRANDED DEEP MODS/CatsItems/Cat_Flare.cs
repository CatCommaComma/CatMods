using Beam;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace CatsItems
{
    public class Cat_Flare : InteractiveObject, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical
    {
        public bool Initial { get { return _initial; } set { _initial = value; } }
        public float DrownAt { get { return _drownAt; } set { _drownAt = value; } }
        public float InitialIntensity { get { return _initialIntensity; } set { _initialIntensity = value; } }
        public Light CurrentLight { get { return _currentLight; } set { _currentLight = value; } }
        public bool Drowning { get { return _drowning; } set { _drowning = value; } }
        public AudioActorHandle AudioHandle { get { return _flareAudioHandle; } }

        protected override void Awake()
        {
            base.Awake();

            _flareLid = gameObject.transform.Find("Flare_Lid");
            _flareSound = GetComponent<AudioSource>();

            if (_flareSound != null) _flareSound.loop = true;
            if (DurabilityPoints <= 0) _flareLid.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }

        public override bool ValidatePrimary(IBase obj)
        {
            if (DurabilityPoints > 0) return true;
            return false;
        }

        private void OnDisable()
        {
            if (_flareLight != null) CleanUp(_flareLight);
        }

        public override void Use()
        {
            if (DurabilityPoints > 0)
            {
                _flareSound.volume = 0.3f;
                _flareSound.rolloffMode = AudioRolloffMode.Custom;
                _flareAudioHandle = AudioManager.GetAudioPlayer().Play3D(_flareSound.clip, transform, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
                _audioSource = AudioManager.GetAudioPlayer().GetAudioSource(_flareAudioHandle);

                _flareLight = Instantiate(Resources.Load<FlareGunProjectile>("Prefabs/StrandedObjects/Other/FLAREBULLET"));

                _flareLight.name = "moddedflare";
                typeof(FlareGunProjectile).GetField("_detonated", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_flareLight, true);

                _flareLid.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

                _flareLight.transform.position = _flareLid.transform.position;
                _flareLight.transform.parent = transform;
                _flareLight.gameObject.SetActive(true);

                DurabilityPoints--;
            }
        }

        public void FadeOutSound(float time)
        {
            if (_audioDiminishing) return;

            StartCoroutine(FadeOut(time));
            _audioDiminishing = true;
        }

        private IEnumerator FadeOut(float time)
        {
            float timeElapsed = 0;

            while (time>timeElapsed)
            {
               timeElapsed += Time.deltaTime;
                float normalizedTime = timeElapsed / time;
                _audioSource.Volume = Mathf.Lerp(1, 0, normalizedTime);

                yield return null;
            }

            _audioSource.Volume = 0;
            AudioManager.GetAudioPlayer().Stop(_flareAudioHandle);
        }

        public void CleanUp(FlareGunProjectile flareLight)
        {
            _initial = false;
            _drownAt = 0f;
            _initialIntensity = 0f;
            _currentLight = null;
            _drowning = false;

            Destroy(flareLight.gameObject);
        }

        private bool _initial = true;
        private float _drownAt = 0f;
        private float _initialIntensity = 0f;
        private Light _currentLight = null;
        private bool _drowning = false;

        private FlareGunProjectile _flareLight;
        private Transform _flareLid = null;

        private AudioSource _flareSound = null;
        private AudioActorHandle _flareAudioHandle = AudioActorHandle.Empty;
        private IAudioSource _audioSource = null;

        private bool _audioDiminishing = false;
    }
}
