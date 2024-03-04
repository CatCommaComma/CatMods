using Beam.Crafting;
using Beam;
using UnityEngine;

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

        public bool CanInteract
        {
            get
            {
                return _canInteract;
            }
            set
            {
                _canInteract = value;
            }
        }

        string IInteractable.GetInteractionDescription(int playerId, IBase obj)
        {
            if (obj == null) return "Press E to mount";

            return default;
        }

        public bool Interact(IPlayer player)
        {
            if (player.Holder.CurrentObject == null)
            {
                MountHarpoon(true, player);
                return true;
            }


            return false;
        }

        public override void OnBeginCrafting()
        {
            base.OnBeginCrafting();
        }

        public bool InteractWithObject(IPlayer player, IBase obj)
        {
            return false;
        }

        protected override void Awake()
        {
            base.Awake();

            _gunHolder = gameObject.transform.Find("GunHolder");
            _gun = gameObject.transform.Find("Gun");
            //Stand
            //GunRotation
            //StandPosition
            //CameraPosition
        }

        private void MountHarpoon(bool mount, IPlayer player)
        {
            isMounted = mount;

            if (mount)
            {

            }
            else
            {

            }
        }

        private void Update()
        {
            if (isMounted)
            {

            }
        }

        private bool _canInteract;
        private bool isMounted = false;

        private Transform _cameraLocation;
        private Transform _gunHolder;
        private Transform _gun;
    }
}