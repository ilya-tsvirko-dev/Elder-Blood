using System;

namespace ElderBlood
{
    // Normalized cooldown fractions (1 = just cast / full cooldown, 0 = ready).
    public readonly struct HudCooldowns
    {
        public readonly float invisibility;
        public readonly float puddle;
        public readonly float bloodSight;

        public HudCooldowns(float invisibility, float puddle, float bloodSight)
        {
            this.invisibility = invisibility;
            this.puddle = puddle;
            this.bloodSight = bloodSight;
        }
    }

    // Bridge ECS → UI. An ECS feed system reads player components and raises these C# events
    // on change; the HUD presenter subscribes. Registered as a VContainer singleton.
    public sealed class HudChannel
    {
        public event Action<float, float> BloodChanged;       // current, capacity
        public event Action<HudCooldowns> CooldownsChanged;

        public void RaiseBlood(float current, float capacity) => BloodChanged?.Invoke(current, capacity);
        public void RaiseCooldowns(in HudCooldowns cooldowns) => CooldownsChanged?.Invoke(cooldowns);
    }
}
