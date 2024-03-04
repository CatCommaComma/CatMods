using Beam;

namespace CatsItems
{
    public class Cat_StarRemover : InteractiveObject
    {
        protected override void Awake()
        {
            base.Awake();
        }

        /*public override void UseOnObject(IBase obj)
        {
            Debug.Log("hit: " + Owner.GameInput.Raycaster.LastHitInfo.collider.gameObject.name);
            base.UseOnObject(obj);
        }*/

        public override void Use()
        {
            //BeamRay ray = Owner.PlayerCamera.GetRay(3f);
            //RaycastHit[] hits = new RaycastHit[4];
            //int num = Physics.SphereCastNonAlloc(ray.OriginPoint, 0.15f, ray.Direction, hits, ray.Length);
            //...
            base.Use();
        }
    }
}
