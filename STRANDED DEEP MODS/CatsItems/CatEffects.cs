using Beam;

namespace CatsItems
{
    public class Vitamins : PlayerEffect, IPlayerEffect
    {
        public static Vitamins Effect { get; private set; }

        public static void Create()
        {
            Effect = new Vitamins();
        }

        public Vitamins() : base("vitaminseffect", "Vitamins", true, 0, 0, 0f, 80f, 4f) { }
    }
}
