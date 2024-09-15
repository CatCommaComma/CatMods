using Beam;

namespace FoodRestoresHealth
{
    public class DrunkEffects
    {
        public class RelaxedEffect : PlayerEffect, IPlayerEffect
        {
            public RelaxedEffect() : base("RELAXED", "RELAXED", true, -25f, -4f, 0f, 20f, 3f)
            {
            }
        }

        public class IntoxicatedEffect : PlayerEffect, IPlayerEffect
        {
            public IntoxicatedEffect() : base("INTOXICATED", "INTOXICATED", false, -45f, 10f, 0f, 0f, 5f)
            {
            }
        }

        public class WastedEffect : PlayerEffect, IPlayerEffect
        {
            public WastedEffect() : base("WASTED", "WASTED", false, -95f, -20f, 0f, -5f, 8f)
            {
            }
        }

        public class AlchoholOverdoseEffect : PlayerEffect, IPlayerEffect
        {
            public AlchoholOverdoseEffect() : base("ALCOHOL OVERDOSE", "ALCOHOL OVERDOSE", false, -80f, -10f, 0f, -37.5f, 8f)
            {
            }
        }

        public class HangoverEffect : PlayerEffect, IPlayerEffect
        {
            public HangoverEffect() : base("HANGOVER", "HANGOVER", false, -35f, -12.5f, 0f, 0f, 4.5f)
            {
            }
        }

        public class HeavyHangoverEffect : PlayerEffect, IPlayerEffect
        {
            public HeavyHangoverEffect() : base("HEAVY HANGOVER", "HEAVY HANGOVER", false, -100f, -20f, 0f, 0f, 9f)
            {
            }
        }

        public class OverdoseRecoveryEffect : PlayerEffect, IPlayerEffect
        {
            public OverdoseRecoveryEffect() : base("OVERDOSE RECOVERY", "OVERDOSE RECOVERY", false, -200f, -60f, 0f, 0f, 12f)
            {
            }
        }


        /***********************************  HALLUCINATION EFFECTS  ****************************************/


        public class HallucinationScared : PlayerEffect, IPlayerEffect
        {
            public HallucinationScared() : base("SCARED", "SCARED", false, 0f, 0f, 0f, 0f, 1.5f)
            {
            }
        }

        public class HallucinationConfused : PlayerEffect, IPlayerEffect
        {
            public HallucinationConfused() : base("CONFUSED", "CONFUSED", false, 0f, 0f, 0f, 0f, 1.5f)
            {
            }
        }

        public class HallucinationLost : PlayerEffect, IPlayerEffect
        {
            public HallucinationLost() : base("LOST", "LOST", false, 0f, 0f, 0f, 0f, 1.5f)
            {
            }
        }

        public class HallucinationPanic : PlayerEffect, IPlayerEffect
        {
            public HallucinationPanic() : base("IN PANIC", "IN PANIC", false, 0f, 0f, 0f, 0f, 1.5f)
            {
            }
        }

        public class HallucinationTerrified : PlayerEffect, IPlayerEffect
        {
            public HallucinationTerrified() : base("TERRIFIED", "TERRIFIED", false, 0f, 0f, 0f, 0f, 1.5f)
            {
            }
        }
    }
}
