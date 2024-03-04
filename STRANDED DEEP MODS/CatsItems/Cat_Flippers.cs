using Beam;
using UnityEngine;
using System.Reflection;
using Beam.Serialization.Json;
using SDPublicFramework;
using System;

namespace CatsItems
{
    public class Cat_Flippers : InteractiveObject, ISaveablePrefab, ISaveableReference, ISaveable, IStorable, IHasCraftingType, IPickupable, IPhysical, IEquippable
    {
        public bool IsEquipped { get { return _equipped; } }
        public static bool IsAnyEquipped { get { return _isEquippedStatic; } }
        public EquippableSystem.EquippableArea EquippableArea { get { return EquippableSystem.EquippableArea.Feet; } }

        private void Update()
        {
            if (IsEquipped)
            {
                transform.position = _player.PlayerCamera.transform.position;
            }
        }

        public override void Use()
        {
            if (Owner.Statistics.HasStatusEffect(StatusEffect.BROKEN_BONES))
            {
                CatUtility.PopNotification("Cannot equip flippers while leg is broken!", 3f);
                return;
            }
            else
            {
                base.Use();
                EquippableSystem.TryEquip(this);
            }
        }

        //ill make it prettier another time
        public void TriggerVisuals(bool active)
        {
            if (active)
            {
                try
                {
                    _flipperProps = new GameObject[2] { null, null };

                    for (int i = 0; i < 2; i++)
                    {
                        _flipperProps[i] = PrefabFactory.InstantiateProp("modded_singleflipper", false);
                    }

                    Transform hips = PlayerRegistry.LocalPlayer.Character.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0);    
                    Transform leftFoot = hips.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
                    Transform rightFoot = hips.GetChild(1).GetChild(0).GetChild(0).GetChild(0);

                    _flipperProps[0].transform.parent = leftFoot;
                    _flipperProps[1].transform.parent = rightFoot;

                    _flipperProps[0].transform.position = leftFoot.position;
                    _flipperProps[1].transform.position = rightFoot.position;
                    //                                                   first is to right/left, second is up/down, third is front/back
                    _flipperProps[0].transform.localPosition = new Vector3(0f, 0.035f, 0.3f);
                    _flipperProps[1].transform.localPosition = new Vector3(0f, 0.035f, 0.3f);

                    _flipperProps[0].transform.rotation = leftFoot.rotation;
                    _flipperProps[1].transform.rotation = rightFoot.rotation;

                    _flipperProps[0].gameObject.SetActive(true);
                    _flipperProps[1].gameObject.SetActive(true);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex);
                }
            }
            else for (int i = 0; i < 2; i++) Destroy(_flipperProps[i]);
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

            if (!initial)
            {
                AudioManager.GetAudioPlayer().Play2D(BasicSounds.EquipSound, AudioMixerChannels.FX, AudioPlayMode.Single);
                _player.Holder.DropCurrent();
            }

            transform.position = _player.PlayerCamera.transform.position;

            rigidbody.useGravity = false;
            rigidbody.detectCollisions = false;

            gameObject.GetComponent<MeshRenderer>().enabled = false;

            ChangePlayerSpeed(1.85f, 3f, 1.2f, 0.85f, 4f, 3.6f, 5.7f, 4.1f, 6.7f);
            TriggerVisuals(true);

            _equipped = true;
        }

        public void OnUnequip()
        {
            _equipped = false;
            AudioManager.GetAudioPlayer().Play2D(BasicSounds.UnequipSound, AudioMixerChannels.FX, AudioPlayMode.Single);

            ResetPlayerSpeed();
            gameObject.GetComponent<MeshRenderer>().enabled = true;

            EquippableSystem.HandleItemUnequip(_player, this);

            rigidbody.useGravity = true;
            rigidbody.detectCollisions = true;

            foreach (Collider coll in this.colliders)
            {
                coll.isTrigger = false;
            }

            _isEquippedStatic = false;
            _player = null;
            TriggerVisuals(false);
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

        private void ResetPlayerSpeed()
        {
            ChangePlayerSpeed(3f, 5.5f, 2f, 1.5f, 2.5f, 1.5f, 4f, 3f, 5f);
        }

        private void ChangePlayerSpeed(float walkSpeed, float runSpeed, float walkBackwardsSpeed, float crouchSpeed, float swimSpeed, float swimFastSpeed, float swimBackwardsSpeed, float diveSpeed, float diveFastSpeed)
        {
            typeof(Movement).GetField("walkSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, walkSpeed);
            typeof(Movement).GetField("runSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, runSpeed);
            typeof(Movement).GetField("walkBackwardSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, walkBackwardsSpeed);
            typeof(Movement).GetField("crouchSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, crouchSpeed);
            typeof(Movement).GetField("swimSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, swimSpeed);
            typeof(Movement).GetField("swimFastSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, swimFastSpeed);
            typeof(Movement).GetField("swimBackwardSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, swimBackwardsSpeed);
            typeof(Movement).GetField("diveSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, diveSpeed);
            typeof(Movement).GetField("diveFastSpeed", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_player.Movement, diveFastSpeed);
        }

        private IPlayer _player;
        private static bool _isEquippedStatic;
        private bool _equipped = false;

        private GameObject[] _flipperProps;
    }
}