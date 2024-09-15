using Beam;

namespace CustomEffects
{
    public class EffectsHolder
    {
        public class SunburnLight : PlayerEffect, IPlayerEffect
        {
            public SunburnLight() : base("SUNBURN_LIGHT", "Light sunburn", false, -35f, -15f, 0f, 0f, 6f)
            {
            }
        }

        public class SunburnHeavy : PlayerEffect, IPlayerEffect
        {
            public SunburnHeavy() : base("SUNBURN_HEAVY", "Heavy sunburn", false, -65f, -30f, 0f, -1f, 14f)
            {
            }
        }

        public class RecentBrokenLeg : PlayerEffect, IPlayerEffect
        {
            public RecentBrokenLeg() : base("RECENT_BROKEN_LEG", "Recent broken leg", true, 0f, 0f, 0f, 0f, 24f)
            {
            }
        }

        public class RecentBrokenLegExtra : PlayerEffect, IPlayerEffect
        {
            public RecentBrokenLegExtra() : base("RECENT_BROKEN_LEG_EXTRA", "Reoccurring broken leg", false, 0f, 0f, 0f, 0f, 48f)
            {
            }
        }
    }
}
