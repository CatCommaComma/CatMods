using Beam.Crafting;
using Beam;
using UnityEngine;
using System.Collections;
using Microsoft.Win32;

namespace CatsItems
{
    public class Cat_Harpoon : Constructing, IInteractable, IConstructable, ICraftable, IBase, IHasCraftingType, IPlaceable, ISelectable
    {
        //rotate holder
        //rotate gun
        //shoot projectiles from gun
        //proper zoom
        //proper camera movement
        //sounds
        //damage creatures
        //build on foundations
        //build on rafts
        //ease in ease out movement
        //unequip if camera is flipped too much
        //take ammo from inventory or load it up beforehand
        //design and make final mesh

        public bool CanInteract { get { return true; } set { CanInteract = value; } }

        protected override void Awake()
        {
            base.Awake();

            //_cameraLocation = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            //_horizontalRotator = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            //_verticalRotator = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
            //_barrelEnd = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
        }

        private void Update()
        {
            if (_mounted)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {

                }
            }
        }

        string IInteractable.GetInteractionDescription(int playerId, IBase obj)
        {
            if (obj == null)
            {
                if (!_mounted) return "Press E to mount";
                else return "Press E to dismount";
            }

            return default;
        }

        public bool Interact(IPlayer player)
        {
            if (player.Holder.CurrentObject == null)
            {
                MountHarpoon(player);
                return true;
            }


            return false;
        }

        public bool InteractWithObject(IPlayer player, IBase obj)
        {
            return false;
        }

        private void MountHarpoon(IPlayer player)
        {
            _mounted = true;
            _user = player;

            _previousParent = player.PlayerCamera.transform.parent;
            _previousPosition = player.PlayerCamera.transform.position;
            _previousRotation = player.PlayerCamera.transform.rotation;

            StartCoroutine(LerpCameraBetween(_previousPosition, _previousRotation, _cameraLocation.position, _cameraLocation.rotation));
        }

        private IEnumerator LerpCameraBetween(Vector3 previous, Quaternion previousR, Vector3 final, Quaternion finalR)
        {
            float timeElapsed = 0f;

            while (1f>timeElapsed)
            {
                timeElapsed+= Time.deltaTime;

                _user.PlayerCamera.transform.position = Vector3.Lerp(previous, final, timeElapsed);
                _user.PlayerCamera.transform.rotation = Quaternion.Lerp(previousR, finalR, timeElapsed);

                yield return null;
            }

            _user.PlayerCamera.transform.position = final;
            _user.PlayerCamera.transform.rotation = finalR;
        }

        private bool _mounted = false;

        private IPlayer _user;

        private Transform _cameraLocation;
        private Transform _horizontalRotator;
        private Transform _verticalRotator;
        private Transform _barrelEnd;

        private Transform _previousParent;
        private Vector3 _previousPosition;
        private Quaternion _previousRotation;
    }
}