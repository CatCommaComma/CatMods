using Beam;
using Beam.Serialization.Json;
using SDPublicFramework;
using UnityEngine;

namespace CatsItems
{
    //i can probably make this inherit from cat_goggles but im too lazy
    public class Cat_GoggleLight : InteractiveObject, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical, IEquippable
    {
        public bool IsEquipped { get { return _equipped; } }
        public static bool IsAnyEquipped { get { return _isEquippedStatic; } }
        public EquippableSystem.EquippableArea EquippableArea {get { return EquippableSystem.EquippableArea.Head; } }

        protected override void Awake()
        {
            base.Awake();
            _flashlightLight = GetComponentInChildren<Light>();

            if (_flashlightLight != null)
            {
                _flashlightLight.enabled = false;
                _flashlightLight.shadows = LightShadows.Soft;
                _flashlightLight.intensity = 1.2f;
                _flashlightLight.range = 22f;
                _flashlightLight.type = LightType.Spot;
                _flashlightLight.spotAngle = 60f;

                _lightPosition = transform.Find("flashlightPosition").gameObject.transform;
            }
        }

        private void Update()
        {
            if (_equipped)
            {
                transform.position = _player.PlayerCamera.transform.position;

                if (Input.GetKeyDown(Main.Settings.ToggleGogglesKey))
                {
                    if (!_lightTurnedOn && _flashlightLight != null)
                    {
                        ToggleLight(true);
                    }
                    else if (_lightTurnedOn)
                    {
                        ToggleLight(false);
                    }
                }
            }
        }

        public override void Use()
        {
            base.Use();
            EquippableSystem.TryEquip(this);
        }

        public void OnEquip(bool initial = false)
        {
            if (!initial) _player = Owner;
            else _player = PlayerRegistry.LocalPlayer;

            _isEquippedStatic = true;

            _lightTurnedOn = false;
            _flashlightLight.enabled = false;

            foreach (Collider coll in colliders)
            {
                coll.isTrigger = true;
            }

            _flashlightLight.transform.position = _player.PlayerCamera.transform.position;
            _flashlightLight.transform.rotation = _player.PlayerCamera.transform.rotation;
            _flashlightLight.transform.parent = _player.PlayerCamera.transform;

            rigidbody.useGravity = false;
            rigidbody.detectCollisions = false;

            if (!initial)
            {
                AudioManager.GetAudioPlayer().Play2D(BasicSounds.EquipSound, AudioMixerChannels.FX, AudioPlayMode.Single);
                _player.Holder.DropCurrent();
            }

            MeshRenderer[] meshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer renderer in meshRenderers)
            {
                renderer.enabled = false;
            }

            transform.position = _player.PlayerCamera.transform.position;
            _equipped = true;
        }

        public void OnUnequip()
        {
            _equipped = false;
            _flashlightLight.enabled = false;

            _flashlightLight.transform.position = _lightPosition.position;
            _flashlightLight.transform.rotation = _lightPosition.rotation;
            _flashlightLight.transform.parent = _lightPosition;

            AudioManager.GetAudioPlayer().Play2D(BasicSounds.UnequipSound, AudioMixerChannels.FX, AudioPlayMode.Single);

            rigidbody.useGravity = true;
            rigidbody.detectCollisions = true;

            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer renderer in meshRenderers)
            {
                renderer.enabled = true;
            }

            EquippableSystem.HandleItemUnequip(_player, this);

            foreach (Collider coll in colliders)
            {
                coll.isTrigger = false;
            }

            _isEquippedStatic = false;
            _player = null;
        }

        private void ToggleLight(bool turnOn)
        {
            _lightTurnedOn = turnOn;
            _flashlightLight.enabled = turnOn;
        }

        public override JObject Save()
        {
            JObject jobject = base.Save();
            jobject.AddField("equipped", JSerializer.Serialize(_equipped));

            return jobject;
        }

        public override void Load(JObject data)
        {
            base.Load(data);
            _equipped = data.GetField("equipped").GetValue<bool>();

            if (_equipped)
            {
                EquippableSystem.TryEquip(this, true);
                gameObject.SetActive(true);
            }
        }

        private IPlayer _player;
        private static bool _isEquippedStatic = false;
        private bool _equipped = false;

        private Light _flashlightLight;
        private bool _lightTurnedOn;
        private Transform _lightPosition;
    }
}