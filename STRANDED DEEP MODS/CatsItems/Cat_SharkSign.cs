using Beam;
using UnityEngine;

namespace CatsItems
{
    public class Cat_SharkSign : InteractiveObject, IAttachable, IAttachingAnchor
    {
        public bool CanAttach { get { return true; } }
        public bool IsAttached { get { return _attached; } }
        public IAttachingAnchor AttachingAnchor { get { return this; } }

        public void Attach(IAttachingAnchor anchor)
        {
            _attached = true;
            DisablePhysics();

            return;
        }

        public void Detach()
        {
            _attached = false;
            OnDetached(this);

            return;
        }

        public Vector3 GetPosition()
        {
            return new Vector3(0, 0, 0.055f);
        }

        public Quaternion GetRotation()
        {
            return new Quaternion(0, 0, 0, 1);
        }

        protected virtual void OnDetached(IAttachable sender)
        {
            AttachableEventHandler<IAttachable> detachedEvent = this.DetachedEvent;

            if (detachedEvent != null)
            {
                detachedEvent(sender);
            }

            DetachedEvent = null;
        }

        public bool IsKinematic()
        {
            return base.IsKinematic;
        }

        public override void OnDestroy()
        {
            DetachedEvent = null;
            base.OnDestroy();
        }

        public override bool Store()
        {
            Detach();
            return base.Store();
        }

        public event AttachableEventHandler<IAttachable> DetachedEvent;
        private bool _attached;
    }
}