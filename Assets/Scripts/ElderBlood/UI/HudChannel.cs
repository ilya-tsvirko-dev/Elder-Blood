using System;

namespace ElderBlood
{
    // One ability's HUD state. Readiness is quantised to the icon's pixel grid upstream,
    // so the fill edge always lands on a whole pixel row.
    public readonly struct HudAbilityState
    {
        public readonly float readiness;   // 0 = just cast, 1 = off cooldown
        public readonly bool affordable;   // player has enough blood to pay the cost

        public HudAbilityState(float readiness, bool affordable)
        {
            this.readiness = readiness;
            this.affordable = affordable;
        }

        public bool Equals(in HudAbilityState other)
        {
            return readiness == other.readiness && affordable == other.affordable;
        }
    }

    public readonly struct HudAbilities
    {
        public readonly HudAbilityState invisibility;
        public readonly HudAbilityState puddle;
        public readonly HudAbilityState bloodSight;

        public HudAbilities(in HudAbilityState invisibility, in HudAbilityState puddle, in HudAbilityState bloodSight)
        {
            this.invisibility = invisibility;
            this.puddle = puddle;
            this.bloodSight = bloodSight;
        }

        public bool Equals(in HudAbilities other)
        {
            return invisibility.Equals(other.invisibility)
                   && puddle.Equals(other.puddle)
                   && bloodSight.Equals(other.bloodSight);
        }
    }

    // Bridge ECS → UI. An ECS feed system reads player components and raises these C# events
    // on change; the HUD presenter subscribes. Registered as a VContainer singleton.
    public sealed class HudChannel
    {
        public event Action<float, float> BloodChanged;       // current, capacity
        public event Action<HudAbilities> AbilitiesChanged;

        public void RaiseBlood(float current, float capacity) => BloodChanged?.Invoke(current, capacity);
        public void RaiseAbilities(in HudAbilities abilities) => AbilitiesChanged?.Invoke(abilities);
    }
}
