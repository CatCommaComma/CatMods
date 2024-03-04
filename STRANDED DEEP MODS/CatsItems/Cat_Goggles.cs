using Beam;
using Beam.Serialization.Json;
using SDPublicFramework;
using UnityEngine;

namespace CatsItems
{
    public class Cat_Goggles : InteractiveObject, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical, IEquippable
    {
        public bool IsEquipped { get { return _equipped; } }
        public static bool IsAnyEquipped { get { return _isEquippedStatic; } }
        public EquippableSystem.EquippableArea EquippableArea { get { return EquippableSystem.EquippableArea.Head; } }

        private void Update()
        {
            if (_equipped)
            {
                transform.position = _player.PlayerCamera.transform.position;
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

            foreach (Collider coll in colliders)
            {
                coll.isTrigger = true;
            }

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
    }
}
