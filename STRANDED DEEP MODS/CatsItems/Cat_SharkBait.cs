using Beam;
using System.Collections;
using UnityEngine;
using Funlabs;
using System;
using Beam.Serialization.Json;
using System.Reflection;

namespace CatsItems
{
    public class Cat_SharkBait : InteractiveObject, IPickupable, IAnimatableSecondary, IChargeable, ISaveable, IUseable, IAnimatablePrimary
    {
        public bool Matured { get { return _matured; } }
        public DateTime TimeCrafted { get { return _timeCrafted; } set { _timeCrafted = value; } }

        public bool CanUseSecondary { get { return true; } set { CanUseSecondary = value; } }
        public bool Reloaded { get { return true; } }

        public AnimationType ReloadAnimation { get { return Idle; } }
        public AnimationType SecondaryIdle { get { return _secondaryIdleAnimation; } }
        public AnimationType Secondary { get { return AnimationType.SPEAR_SECONDARY; } }

        public BaseActionUnityEvent Charged { get { return _charged; } }
        public BaseActionUnityEvent UsedSecondary { get { return _usedSecondary; } }

        protected override void Awake()
        {
            if (!_matured) DisplayNamePrefixes.AddOrIgnore("Fresh", -2);
            base.Awake();
        }

        private void Update()
        {
            if (!_attractingSharks && !IsPickedUp && transform.transform.position.y < 0.1f)
            {
                StartCoroutine(AttractSharks());
            }

            //if this turns out to be very inefficient, i'll change it
            if (!_matured && TimeCrafted.AddDays(3) > GameTime.Now) Mature();
        }

        public void Mature()
        {
            if (!_matured)
            {
                typeof(InteractiveObject).GetField("_descriptionTerm", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, "Useful for attracting aggressive sharks in the open ocean. Unbearably vile stench.");

                for (int i = 0; i < this.renderers.Length; i++)
                {
                    renderers[i].GetPropertyBlock(PropertyBlock);
                    //PropertyBlock.SetColor("_Color", new Color(0.83f, 1f, 0.73f, 1f));
                    PropertyBlock.SetColor("_Color", new Color(0.76f, 1f, 0.66f, 1f));
                    renderers[i].SetPropertyBlock(PropertyBlock);
                }

                ParticleManager.CreateParticle(Singleton<ParticleManager>.Instance.PARTICLE_FOOD_SPOIL, base.transform);

                DisplayNamePrefixes.Remove("Fresh");
                DisplayNamePrefixes.AddOrIgnore("Matured", -2);
                _matured = true;
            }
        }

        public void PreSecondary()
        {
        }

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
                rigidbody.AddForce(transform.forward * 8, ForceMode.Impulse);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void PostSecondary()
        {
        }

        public override void Hold(bool holding)
        {
            base.Hold(holding);

            if (_matured)
            {
                if (holding)
                {
                    Owner.Statistics.ApplyStatusEffect(Stench.Effect);
                }
                else
                {
                    Owner.Statistics.RemoveStatusEffect(Stench.Effect);
                }
            }
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
            int randomNumber = UnityEngine.Random.Range(8, 17);

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

            int n = _matured? UnityEngine.Random.Range(3, 7) : 1;

            for (int i=0; i<n; i++)
            {
                uint sharkId = GetSharkId(UnityEngine.Random.Range(0, 4));
                SaveablePrefab prefab = MultiplayerMng.Instantiate<SaveablePrefab>(sharkId, RandomSpawnLocation(), Quaternion.identity, MiniGuid.New(), null);

                prefab.gameObject.SetActive(true);
                prefab.gameObject.GetComponent<Piscus_Creature>().CurrentAttackTarget = Owner;

                yield return new WaitForSeconds(1f);
            }

            gameObject.GetComponent<Buoyancy>().enabled = false;
            rigidbody.drag = 2;

            yield return new WaitForSeconds(6f);

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

        public override JObject Save()
        {
            JObject jobject = base.Save();

            jobject.AddField("IsMatured", _matured);
            jobject.AddField("FreshCraft", JSerializer.Serialize(_timeCrafted));

            return jobject;
        }

        public override void Load(JObject data)
        {
            base.Load(data);

            try
            {
                JObject field = data.GetField("IsMatured");
                bool matured = field.GetValue<bool>();
                if (matured) Mature();

                JObject field2 = data.GetField("FreshCraft");
                _timeCrafted = JSerializer.Deserialize<DateTime>(field2);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

        private AnimationType _secondaryIdleAnimation = AnimationType.ONEHANDED_LARGE_IDLE; //AnimationType.SPEAR_SECONDARY_IDLE; //AnimationType.ONEHANDED_LARGE_IDLE; //AnimationType.ONEHANDED_LARGE_IDLE; //AnimationType.SPEAR_SECONDARY_IDLE;
        private bool _attractingSharks = false;

        private BaseActionUnityEvent _charged = null;
        private BaseActionUnityEvent _usedSecondary = null;

        private DateTime _timeCrafted;
        private bool _matured = false;
    }
}