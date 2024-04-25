using Beam;
using SDPublicFramework;

namespace CatsItems
{
    public class Cat_Vitamins : InteractiveObject_MEDICAL, ICustomEffect
    {
        public PlayerEffect CustomEffect { get { return Vitamins.Effect; } }
    }
}
