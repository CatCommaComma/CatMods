using Beam;
using Beam.Serialization.Json;
using UnityEngine;
using System.Reflection;

namespace CatsItems
{
    public class Cat_Bucket : InteractiveObject_FOOD, IBase, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical
    {
        public bool IsSalty { get { return this._isSalty; } }

        protected override void Awake()
        {
            base.Awake();

            Servings = 0;
            _waterPlane = gameObject.transform.Find("Bucket_Water");
            _fillPoint = gameObject.transform.Find("Bucket_FillPosition");

            _boilSound = GetComponentInChildren<AudioSource>();
            _boilSound.loop = true;
            _boilSound.rolloffMode = AudioRolloffMode.Custom;

            PlayerRegistry.LocalPlayer.Movement.Jumped += PlayerJumped;
        }

        private void Update()
        {
            if (!LevelLoader.IsLoading()) //i don't know if i actually need this check
            {
                if (Servings == 0)
                {
                    if (_waterPlane.gameObject.activeInHierarchy)
                    {
                        _waterPlane.gameObject.SetActive(false);
                    }

                    _isSalty = false;
                }
                else
                {
                    if (!_waterPlane.gameObject.activeInHierarchy) _waterPlane.gameObject.SetActive(true);
                    float waterPlanePosition = (Servings / 5f);
                    float waterPlaneSize = 1f - (0.25f - 0.25f * waterPlanePosition);

                    _waterPlane.localPosition = new Vector3(0f, -(0.24f * (1f - waterPlanePosition)), 0f);
                    _waterPlane.localScale = new Vector3(waterPlaneSize, waterPlaneSize, waterPlaneSize);
                }

                if (_fillPoint.position.y < 0f && Servings != 5)
                {
                    typeof(Cooking).GetField("_cookingHours", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Cooking, 2f);
                    Servings = 5;
                    _isSalty = true;
                }
                else
                {
                    if (!IsPickedUp)
                    {
                        float tilt = CheckTilt();

                        if (Singleton<AtmosphereStorm>.Instance.Rain > 0 && tilt > 0.5f && Servings < 5)
                        {
                            _rainFill += Time.deltaTime;

                            if (_rainFill >= SECONDS_TO_REFILL_RAIN)
                            {
                                CollectRain();
                            }
                        }
                    }
                }
                CheckWater();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            PlayerRegistry.LocalPlayer.Movement.Jumped -= PlayerJumped;
        }

        public override string GetDisplayName()
        {
            CheckWater();
            return DisplayName;
        }

        public override bool InteractWithObject(IPlayer player, IBase obj)
        {
            InteractiveObject_FOOD waterContainer = obj as InteractiveObject_FOOD;

            if (waterContainer != null && !_isSalty && waterContainer.Servings < waterContainer.OriginalServings && waterContainer.Hydration > 0 && Servings > 0)
            {
                waterContainer.Servings++;
                Servings--;

                return true;
            }

            return false;
        }

        public override void Use()
        {
            if (Servings > 0 && !Owner.Movement.IsBusy)
            {
                float vomitChance = this.IsSalty ? 100f : 0f;
                Owner.Statistics.Eat(InteractiveType.FOOD_WATER_SKIN, MeatProvenance.Other, base.Calories, 0, 0, 0, base.HydrationPerServe, vomitChance);

                Servings--;
            }
        }

        public override void Hold(bool holding)
        {
            if (holding) StopBoiling();
            base.Hold(holding);
        }

        public override bool ValidatePrimary(IBase obj)
        {
            if (Servings > 0 && !Owner.Movement.IsBusy)
            {
                return base.ValidatePrimary(obj);
            }
            return false;
        }

        private void PlayerJumped()
        {
            if (IsPickedUp && Servings > 3)
            {
                Servings--;
                _waterParticles.Emit(4);
            }
        }

        private void CollectRain()
        {
            Servings++;
            _rainFill = 0;
        }

        public void CheckWater()
        {
            if (Servings > 0)
            {
                DisplayNamePrefixes.Remove("ITEM_DISPLAY_NAME_PREFIX_EMPTY");
                DisplayNamePrefixes.Remove(!_isSalty ? "Sea Water" : "Fresh Water");
                string prefix = this._isSalty ? "Sea Water" : "Fresh Water";
                DisplayNamePrefixes.AddOrIgnore(prefix, -2);
            }
            else
            {
                DisplayNamePrefixes.Remove("Sea Water");
                DisplayNamePrefixes.Remove("Fresh Water");
                DisplayNamePrefixes.AddOrIgnore("ITEM_DISPLAY_NAME_PREFIX_EMPTY", -2);
            }
            base.OnDisplayNameChanged();
        }

        private float CheckTilt()
        {
            float tiltPercentage = Vector3.Dot(transform.up, Vector3.up);

            //there's definitely a prettier way to do this
            if (tiltPercentage < 0.5f && base.Servings == 5)
            {
                Servings--;
                _waterParticles.Emit(2);
            }
            else if (tiltPercentage < 0.45f && Servings == 4)
            {
                Servings--;
                _waterParticles.Emit(2);
            }
            else if (tiltPercentage < 0.40f && Servings == 3)
            {
                Servings--;
                _waterParticles.Emit(2);
            }
            else if (tiltPercentage < 0.35f && Servings == 2)
            {
                Servings--;
                _waterParticles.Emit(2);
            }
            else if (tiltPercentage < 0.30f && Servings == 1)
            {
                Servings--;
                _waterParticles.Emit(2);
                CheckWater();
            }

            return tiltPercentage;
        }

        public void BoilToFresh()
        {
            _isSalty = false;
            CheckWater();
        }

        public void StartBoiling()
        {
            if (!_audioHandleEnabled)
            {
                _boilAudioHandle = AudioManager.GetAudioPlayer().Play3D(_boilSound.clip, transform.position, AudioMixerChannels.FX, AudioRollOffDistance.Near, AudioPlayMode.Persistent);
                _audioHandleEnabled = true;
                Cooking.IsBeingCooked = true;
            }
        }

        public void StopBoiling()
        {
            AudioManager.GetAudioPlayer().Stop(_boilAudioHandle);
            _boilSound.Stop();

            _boilAudioHandle = AudioActorHandle.Empty;
            _audioHandleEnabled = false;
            Cooking.IsBeingCooked = false;
        }

        public override JObject Save()
        {
            JObject jobject = base.Save();

            jobject.AddField("IsSalty", _isSalty);
            jobject.AddField("RainFill", _rainFill);

            gameObject.SetActive(true);

            return jobject;
        }

        public override void Load(JObject data)
        {
            base.Load(data);

            JObject field = data.GetField("IsSalty");
            _isSalty = field.GetValue<bool>();

            JObject field2 = data.GetField("RainFill");
            _rainFill = field2.GetValue<float>();

            CheckWater();
        }

        private Transform _fillPoint;
        private Transform _waterPlane;

        private ParticleSystem _waterParticles;

        private bool _isSalty = false;
        private float _rainFill = 0;

        private AudioActorHandle _boilAudioHandle = AudioActorHandle.Empty;
        private AudioSource _boilSound = null;
        private bool _audioHandleEnabled = false;

        private const int SECONDS_TO_REFILL_RAIN = 200;
    }
}