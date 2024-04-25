using Beam;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CatsItems
{
    public class Cat_StarRemover : InteractiveObject
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public override void Use()
        {
            if (Physics.Raycast(Owner.PlayerCamera.transform.position, Owner.PlayerCamera.transform.forward, out RaycastHit hit, 15f, ~(1 << LayerMask.NameToLayer("Player"))) && hit.collider != null && hit.collider.gameObject != null)
            {
                StartCoroutine(KillUrchin(hit.collider.gameObject.GetComponent<SeaUrchins>()));
            }
            base.Use();
        }

        private IEnumerator KillUrchin(SeaUrchins urchin)
        {
            if (urchin == null || urchin.gameObject.name.ToLower().Contains("trigger")) yield break;

            GameObject urchinObject = urchin.gameObject;
            Destroy(urchin);

            DurabilityPoints -= 1;
            AudioManager.GetAudioPlayer().Play2D(LocalEmbeddedAudio.SlimyHit, AudioMixerChannels.Voice, AudioPlayMode.Single);

            if (DurabilityPoints < 0.2f)
            {
                Owner.Holder.DropCurrent();
                this.CanPickUp = false;
                this.CanDrag = false;
                this.CanInteract = false;
            }

            GameObject destroyer = new GameObject("urchinDestroyer");
            destroyer.AddComponent<UrchinDecayer>().StartDecay(urchinObject);

            if (DurabilityPoints < 0.2f)
            {
                this.colliders[0].enabled = false;
                yield return new WaitForSecondsRealtime(2f);
                Destroy(this.gameObject);
            }
        }
    }

    public class UrchinDecayer : MonoBehaviour
    {
        public void StartDecay(GameObject urchin)
        {
            StartCoroutine(Decay(urchin));
        }

        private IEnumerator Decay(GameObject urchin)
        {
            float timeElapsed = 0;
            BannedRespawnPositions.Add(urchin.transform.position);

            Renderer[] renderer = urchin.GetComponentsInChildren<Renderer>();
            if (renderer.Length == 0) renderer = new Renderer[] { urchin.GetComponent<Renderer>() };
            if (renderer[0] != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();

                while (4f > timeElapsed)
                {
                    timeElapsed += Time.deltaTime;

                    float color = Mathf.Lerp(1, 0.35f, timeElapsed * 0.25f);

                    for (int i = 0; i < renderer.Length; i++)
                    {
                        renderer[i].GetPropertyBlock(block);
                        block.SetColor("_Color", new Color(color, color, color, 1f));
                        renderer[i].SetPropertyBlock(block);
                    }

                    yield return null;
                }

                for (int i = 0; i < renderer.Length; i++)
                {
                    renderer[i].GetPropertyBlock(block);
                    block.SetColor("_Color", new Color(0.35f, 0.35f, 0.35f, 1f));
                    renderer[i].SetPropertyBlock(block);
                }
            }

            Destroy(this.gameObject);
        }

        public static List<Vector3> BannedRespawnPositions = new List<Vector3>();
    }
}
