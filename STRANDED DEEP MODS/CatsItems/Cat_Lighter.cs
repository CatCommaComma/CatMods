using Beam;
using UnityEngine;
using Beam.Utilities;
using System.Reflection;
using Beam.Crafting;
using SDPublicFramework;
using System.Collections.Generic;
using System;

namespace CatsItems
{
    public class Cat_Lighter : InteractiveObject, IFlammable
    {
        protected override void Awake()
        {
            base.Awake();

            _lighterLid = base.gameObject.transform.Find("Lid");
            _lighterFire = GetComponentInChildren<ParticleSystem>();

            _lighterLight = GetComponentInChildren<Light>();
            _lighterLight.range = 1f;
            _lighterLight.intensity = 0.7f;
            _lighterLight.enabled = false;

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
            if (fuelCan != null && this.DurabilityPoints != this.OriginalDurabilityPoints)
            {
                if (fuelCan.Fuel >= ((this.OriginalDurabilityPoints - this.DurabilityPoints) * 0.015f))
                {
                    float newfuel = fuelCan.Fuel - ((this.OriginalDurabilityPoints - this.DurabilityPoints) * 0.015f);

                    fi_fuel.SetValue(fuelCan, newfuel);

                    this.DurabilityPoints = this.OriginalDurabilityPoints;
                    return true;
                }
                else
                {
                    CatUtility.PopNotification("Not enough fuel to refill the lighter!", 2.5f);
                    return false;
                }
            }
            return false;
        }

        //Triggered once when equipped and when unequipped
        public override void Hold(bool holding)
        {
            base.Hold(holding);
            if (holding)
            {
                LeanTween.cancel(base.gameObject);
                //_lighterLid.localRotation = Quaternion.identity;
                LeanTween.value(base.gameObject, new Action<float>(this.AnimateLid), 10f, -99f, 0.45f).setEase(LeanTweenType.easeInOutQuad);
                AudioManager.GetAudioPlayer().Play2D(LocalEmbeddedAudio.OpenLighterSound, AudioMixerChannels.Voice, AudioPlayMode.Single);

                if (this.DurabilityPoints > 0.1)
                {
                    _lighterFire.Play();
                    _lighterLight.enabled = true;
                }
            }
            else
            {
                LeanTween.cancel(base.gameObject);
                CleanUpLighterFire();
                LeanTween.value(base.gameObject, new Action<float>(this.AnimateLid), -99f, 10f, 0.3f).setEase(LeanTweenType.linear);
                _lighterLight.enabled = false;
            }
        }

        private void Update()
        {
            if (IsPickedUp)
            {
                if (DurabilityPoints > 0.1)
                {
                    if (_lighterLid.position.y < 0f && !_lighterFire.isStopped)
                    {
                        CleanUpLighterFire();
                        _lighterLight.enabled = false;
                    }
                    else if (_lighterLid.position.y > 0.1f && _lighterFire.isStopped)
                    {
                        _lighterFire.Play();
                        _lighterLight.enabled = true;
                    }
                }
                else if (!_lighterFire.isStopped)
                {
                    CleanUpLighterFire();
                }
            }
        }

        private void CleanUpLighterFire()
        {
            _lighterFire.Stop();
            _lighterFire.Clear();
        }

        private void AnimateLid(float rotation)
        {
            _lighterLid.localRotation = Quaternion.Euler(rotation * new Vector3(0f, 0f, 1f));
        }

        private Transform _lighterLid;
        private ParticleSystem _lighterFire;
        private Light _lighterLight;

        private static FieldInfo fi_fuel = typeof(InteractiveObject_FUELCAN).GetField("_fuel", BindingFlags.Instance | BindingFlags.NonPublic);

        public bool IsBurning => (DurabilityPoints >= 0.2f);
    }
}
