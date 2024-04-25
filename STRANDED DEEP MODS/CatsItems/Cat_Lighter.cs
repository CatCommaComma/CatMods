using Beam;
using UnityEngine;
using Beam.Utilities;
using System.Reflection;
using Beam.Crafting;
using System;

namespace CatsItems
{
    public class Cat_Lighter : InteractiveObject, IFlammable
    {
        protected override void Awake()
        {
            base.Awake();

            _lighterLid = base.gameObject.transform.Find("Lid");
            _lighterFire = transform.Find("Fire").GetComponent<ParticleSystem>();
            _lighterSpark = transform.Find("Spark").GetComponent<ParticleSystem>();

            _lighterLight = transform.Find("Light").GetComponent<Light>();
            _lighterLight.range = 0.9f;
            _lighterLight.intensity = 0.65f;

            CleanUpLighterFire();
        }

        public override string GetInteractionDescription(int playerId, IBase obj)
        {
            if (obj.IsNullOrDestroyed())
            {
                return string.Empty;
            }

            if ((obj as InteractiveObject_FUELCAN) != null)
            {
                return "ITEM_INTERACTION_DESCRIPTION_REFILL";
            }

            return string.Empty;
        }

        public override bool InteractWithObject(IPlayer player, IBase obj)
        {
            if (obj.IsNullOrDestroyed())
            {
                return false;
            }

            InteractiveObject_FUELCAN fuelCan = obj as InteractiveObject_FUELCAN;
            if (fuelCan != null && this.DurabilityPoints != this.OriginalDurabilityPoints && fuelCan.Fuel >= 0.003f)
            {
                float maxRefill = (this.OriginalDurabilityPoints - this.DurabilityPoints) * 0.003f;
                float newfuel = fuelCan.Fuel - maxRefill;

                if (fuelCan.Fuel < maxRefill)
                {
                    maxRefill+=newfuel;
                    newfuel = 0f;
                }

                fi_fuel.SetValue(fuelCan, newfuel);
                this.DurabilityPoints+= (maxRefill / 0.003f);
                AudioManager.GetAudioPlayer().Play2D(LocalEmbeddedAudio.FillLighter, AudioMixerChannels.Voice, AudioPlayMode.Single);
                return true;
            }
            return false;
        }

        //Triggered once when equipped and when unequipped
        public override void Hold(bool holding)
        {
            base.Hold(holding);
            _isBurning = false;

            if (holding)
            {
                LeanTween.cancel(base.gameObject);
                LeanTween.value(base.gameObject, new Action<float>(this.AnimateLid), 10f, -99f, 0.45f).setEase(LeanTweenType.easeInOutQuad);
                AudioManager.GetAudioPlayer().Play2D(LocalEmbeddedAudio.OpenLighterSound, AudioMixerChannels.Voice, AudioPlayMode.Single);
            }
            else
            {
                LeanTween.cancel(base.gameObject);
                CleanUpLighterFire();
                LeanTween.value(base.gameObject, new Action<float>(this.AnimateLid), -99f, 10f, 0.3f).setEase(LeanTweenType.linear);
            }
        }

        private void Update()
        {
            if (IsPickedUp && IsBurning)
            {
                if (DurabilityPoints < 0.1f || _lighterLid.position.y < 0f)
                {
                    _isBurning = false;
                    CleanUpLighterFire();
                    return;
                }

                DurabilityPoints -= (Time.deltaTime * 0.1f);
            }
        }

        public override void Use()
        {
            _isBurning = !_isBurning && _lighterLid.position.y > 0f;

            if (_isBurning)
            {
                _lighterSpark.Play();
                AudioManager.GetAudioPlayer().Play2D(LocalEmbeddedAudio.LighterStrikeSound, AudioMixerChannels.Voice, AudioPlayMode.Single);

                if (DurabilityPoints <= 30)
                {
                    if (UnityEngine.Random.Range(0, DurabilityPoints) <= 1)
                    {
                        _isBurning = false;
                        return;
                    }
                }

                _lighterFire.Play();
                _lighterLight.enabled = true;
            }
            else
            {
                CleanUpLighterFire();
            }
        }

        private void CleanUpLighterFire()
        {
            _lighterSpark.Stop();
            _lighterSpark.Clear();
            _lighterFire.Stop();
            _lighterFire.Clear();
            _lighterLight.enabled = false;
        }

        private void AnimateLid(float rotation)
        {
            _lighterLid.localRotation = Quaternion.Euler(rotation * new Vector3(0f, 0f, 1f));
        }

        private Transform _lighterLid;
        private ParticleSystem _lighterFire;
        private ParticleSystem _lighterSpark;
        private Light _lighterLight;

        private static FieldInfo fi_fuel = typeof(InteractiveObject_FUELCAN).GetField("_fuel", BindingFlags.Instance | BindingFlags.NonPublic);

        public bool IsBurning => (_isBurning);
        private bool _isBurning = false;
    }
}
