using Beam;
using System.Collections;
using UnityEngine;
using Funlabs;
using System.Collections.Generic;
using System;

namespace CatsItems
{
    public class Cat_SharkBait : InteractiveObject, IPickupable, IAnimatableSecondary, IChargeable, ISaveable, IUseable, IAnimatablePrimary
    {
        public bool CanUseSecondary { get { return true; } set { CanUseSecondary = value; } }
        public bool Reloaded { get { return true; } }

        public AnimationType ReloadAnimation { get { return _secondaryIdleAnimation; } }
        public AnimationType SecondaryIdle { get { return _secondaryIdleAnimation; } }
        public AnimationType Secondary { get { return AnimationType.SPEAR_SECONDARY; } }

        public BaseActionUnityEvent Charged { get { return _charged; } }
        public BaseActionUnityEvent UsedSecondary { get { return _usedSecondary; } }

        private void Update()
        {
            if (!_attractingSharks && !IsPickedUp && transform.transform.position.y < 0.1f)
            {
                StartCoroutine(AttractSharks());
            }
        }

        public void PreSecondary()
        {
            /*ActionEventData.Populate(this, null);
            _charged.Invoke(this, ActionEventData);*/
        }

        public void UseSecondary()
        {
            try
            {
                /*ActionEventData.Populate(this, null);
                ActionEventData.Position = Owner.transform.position;
                _usedSecondary.Invoke(this, ActionEventData);*/

                BeamRay ray = Owner.PlayerCamera.GetRay(7f);
                Owner.Holder.DropCurrent();

                float distance = Vector3.Distance(transform.position, ray.OriginPoint);

                Transform baitTransform = transform.transform;

                baitTransform.position = ray.OriginPoint;
                baitTransform.LookAt(ray.EndPoint);
                baitTransform.position += baitTransform.forward * distance;
                rigidbody.AddForce(transform.forward * 9, ForceMode.Impulse);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void PostSecondary()
        {
        }

        private IEnumerator AttractSharks()
        {
            _attractingSharks = true;

            if (Singleton<StrandedWorld>.Instance.InBounds(transform.position))
            {
                yield return new WaitForSeconds(2f);
                _attractingSharks = false;
                yield break;
            }

            rigidbody.drag = 9;
            int randomNumber = UnityEngine.Random.Range(5, 15);

            CanPickUp = false;
            CanDrag = false;

            yield return new WaitForSeconds(randomNumber);

            if (Singleton<StrandedWorld>.Instance.InBounds(transform.position))
            {
                CanPickUp = true;
                CanDrag = true;
                _attractingSharks = false;
                yield break;
            }

            uint sharkId = GetSharkId(UnityEngine.Random.Range(0, 4));

            SaveablePrefab prefab = MultiplayerMng.Instantiate<SaveablePrefab>(sharkId, RandomSpawnLocation(), Quaternion.identity, MiniGuid.New(), null);

            prefab.gameObject.SetActive(false);

            gameObject.GetComponent<Buoyancy>().enabled = false;
            rigidbody.drag = 2;

            yield return new WaitForSeconds(7f);

            prefab.gameObject.SetActive(true);

            Destroy(this.gameObject);
        }

        private Vector3 RandomSpawnLocation()
        {
            bool add1 = UnityEngine.Random.Range(0, 2) == 1 ? true : false;
            bool add2 = UnityEngine.Random.Range(0, 2) == 1 ? true : false;

            float location1 = transform.transform.position.x + (add1 ? 25 : -25);
            float location2 = transform.transform.position.z + (add2 ? 25 : -25);

            return new Vector3(location1, -20, location2);
        }

        private uint GetSharkId(int chosen)
        {
            switch (chosen)
            {
                case 0:
                    return 333U; //goblin

                case 1:
                    return 11U; //great white

                case 2:
                    return 331U; //hammerhead

                case 3:
                    return 10U; //tiger

                    //case 2:
                    //return 151U; //marlin

                    //case 5:
                    //return 332U; //whale shark
            }

            return 0U;
        }

        public bool CanReloadWith(IPickupable pickupable)
        {
            return false;
        }

        public void Reload(IPickupable pickupable)
        {
        }

        public bool ValidateSecondary(IBase obj)
        {
            return true;
        }

        private AnimationType _secondaryIdleAnimation = AnimationType.ONEHANDED_LARGE_IDLE; //AnimationType.SPEAR_SECONDARY_IDLE;
        private bool _attractingSharks = false;

        private BaseActionUnityEvent _charged = null;
        private BaseActionUnityEvent _usedSecondary = null;
    }
}