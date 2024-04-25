using Beam;
using Beam.Serialization.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CatsItems
{
    //flare is shitcode, needs to be rewritten
    public class Cat_Flare : InteractiveObject, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical, IAnimatableSecondary, IChargeable
    {
        public bool Initial { get { return _initial; } set { _initial = value; } }
        public float DrownAt { get { return _drownAt; } set { _drownAt = value; } }
        public float InitialIntensity { get { return _initialIntensity; } set { _initialIntensity = value; } }
        public Light CurrentLight { get { return _currentLight; } set { _currentLight = value; } }
        public bool Drowning { get { return _drowning; } set { _drowning = value; } }
        public AudioActorHandle AudioHandle { get { return _flareAudioHandle; } }
        public Transform Lid { get { return _flareLid; } }

        public bool CanUseSecondary { get { return true; } set { CanUseSecondary = value; } }
        public bool Reloaded { get { return true; } }
        public BaseActionUnityEvent Charged { get { return _charged; } }
        public BaseActionUnityEvent UsedSecondary { get { return _usedSecondary; } }

        public AnimationType ReloadAnimation { get { return Idle; } }
        public AnimationType SecondaryIdle { get { return _secondaryIdleAnimation; } }
        public AnimationType Secondary { get { return AnimationType.SPEAR_SECONDARY; } }

        protected override void Awake()
        {
            base.Awake();

            _flareLid = gameObject.transform.Find("Flare_Lid");
            _flareTop = gameObject.transform.Find("Top").GetComponent<Renderer>().material;
            _flareSound = GetComponent<AudioSource>();

            _flareSparks = transform.Find("Particles").GetChild(0).GetComponent<ParticleSystem>();
            _flareSparks.Stop();
            _flareSparks.Clear();

            _flareEffect = transform.Find("Particles").GetChild(1).GetComponent<ParticleSystem>();
            _flareEffect.Stop();
            _flareEffect.Clear();

            if (_flareSound != null) _flareSound.loop = true;
            if (DurabilityPoints <= 0.1f) _flareLid.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
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

                _flareTop.color = Color.white;

                _flareLight = Instantiate(Resources.Load<FlareGunProjectile>("Prefabs/StrandedObjects/Other/FLAREBULLET"));
                _flareLight.name = "moddedflare";
                typeof(FlareGunProjectile).GetField("_detonated", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_flareLight, true);

                _flareLid.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

                _flareLight.transform.position = _flareLid.transform.position;
                _flareLight.transform.parent = transform;
                _flareLight.gameObject.SetActive(true);

                _flareSparks.gameObject.transform.parent.gameObject.SetActive(true);

                _flareSparks.Play();
                _flareEffect.gameObject.SetActive(false);
                //_flareEffect.Play();

                DurabilityPoints--;
            }
        }

        private static void PrintAllAll(Transform parent)
        {
            SDPublicFramework.CatUtility.PrintAll(parent.gameObject);

            for (int i = 0; i < parent.childCount; i++)
            {
                PrintAllAll(parent.GetChild(i));
            }
        }

        public void PreSecondary() {}

        public void UseSecondary()
        {
            try
            {
                BeamRay ray = Owner.PlayerCamera.GetRay(7f);
                Owner.Holder.DropCurrent();

                float distance = Vector3.Distance(transform.position, ray.OriginPoint);

                Transform baitTransform = transform.transform;

                baitTransform.position = ray.OriginPoint;
                baitTransform.LookAt(ray.EndPoint);
                baitTransform.position += baitTransform.forward * distance;
                rigidbody.AddForce(transform.forward * 7, ForceMode.Impulse);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void PostSecondary() {}

        public void FadeOutSound(float time)
        {
            if (_audioDiminishing) return;

            StartCoroutine(FadeOut(time));
            _audioDiminishing = true;
        }

        private IEnumerator FadeOut(float time)
        {
            float timeElapsed = 0;

            _flareEffect.Stop();
            _flareSparks.Stop();

            while (time>timeElapsed)
            {
                timeElapsed += Time.deltaTime;
                float normalizedTime = timeElapsed / time;
                _audioSource.Volume = Mathf.Lerp(1, 0, normalizedTime);
                _flareTop.color = Color.Lerp(Color.white, Color.black, normalizedTime);

                yield return null;
            }

            _flareSparks.gameObject.transform.parent.gameObject.SetActive(false);
            _flareTop.color = Color.black;

            _audioSource.Volume = 0;
            AudioManager.GetAudioPlayer().Stop(_flareAudioHandle);
        }

        public void CleanUp(FlareGunProjectile flareLight)
        {
            if (_flareSparks != null) _flareSparks.gameObject.transform.parent.gameObject.SetActive(false);
            if (_flareAudioHandle != null) AudioManager.GetAudioPlayer().Stop(_flareAudioHandle);
            if (_flareTop != null) _flareTop.color = Color.black;

            _initial = false;
            _drownAt = 0f;
            _initialIntensity = 0f;
            _currentLight = null;
            _drowning = false;

            Destroy(flareLight.gameObject);
        }

        public override void Load(JObject data)
        {
            base.Load(data);
            if (DurabilityPoints <= 0.1f) _flareLid.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        }

        public bool CanReloadWith(IPickupable pickupable)
        {
            return false;
        }

        public void Reload(IPickupable pickupable) {}

        public bool ValidateSecondary(IBase obj)
        {
            return true;
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

        private Material _flareTop;

        private ParticleSystem _flareSparks;
        private ParticleSystem _flareEffect;

        private AnimationType _secondaryIdleAnimation = AnimationType.ONEHANDED_LARGE_IDLE; //AnimationType.SPEAR_SECONDARY_IDLE;

        private BaseActionUnityEvent _charged = null;
        private BaseActionUnityEvent _usedSecondary = null;
    }
}
