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

        public Vitamins() : base("vitaminseffect", "Vitamins", true, 0, 0, 0f, 60f, 4f) { }
    }

    public class Stench : PlayerEffect, IPlayerEffect
    {
        public static Stench Effect { get; private set; }

        public static void Create()
        {
            Effect = new Stench();
        }

        public Stench() : base("stencheffect", "Stench", false, 0, 0, 0f, 0f) { }
    }
}
