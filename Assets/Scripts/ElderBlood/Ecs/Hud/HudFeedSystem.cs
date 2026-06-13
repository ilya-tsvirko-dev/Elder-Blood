using ElderBlood.Configs;
using ElderBlood.Ecs.Abilities;
using ElderBlood.Ecs.Blood;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Hud
{
    // Reads the player's blood and ability cooldowns each frame and pushes changes to the HUD
    // via HudChannel. Polls components internally (cheap), fires C# events only on change.
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HudFeedSystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<BloodComponent> _bloodStash;
        private Stash<AbilitiesComponent> _abilitiesStash;

        private readonly HudChannel _channel;
        private readonly AbilityConfig _config;

        private float _lastCurrent = float.NaN;
        private float _lastCapacity = float.NaN;
        private float _lastInvisibility = float.NaN;
        private float _lastPuddle = float.NaN;
        private float _lastBloodSight = float.NaN;

        public HudFeedSystem(HudChannel channel, MainConfig config)
        {
            _channel = channel;
            _config = config.AbilityConfig;
        }

        public void OnAwake()
        {
            _filter = World.Filter.With<BloodComponent>().With<AbilitiesComponent>().Build();
            _bloodStash = World.GetStash<BloodComponent>();
            _abilitiesStash = World.GetStash<AbilitiesComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var blood = ref _bloodStash.Get(entity);
                if (blood.current != _lastCurrent || blood.capacity != _lastCapacity)
                {
                    _lastCurrent = blood.current;
                    _lastCapacity = blood.capacity;
                    _channel.RaiseBlood(blood.current, blood.capacity);
                }

                ref var abilities = ref _abilitiesStash.Get(entity);
                var invisibility = Fraction(abilities.invisibilityCooldown, _config.invisibilityCooldown);
                var puddle = Fraction(abilities.puddleCooldown, _config.puddleCooldown);
                var bloodSight = Fraction(abilities.bloodSightCooldown, _config.bloodSightCooldown);

                if (invisibility != _lastInvisibility || puddle != _lastPuddle || bloodSight != _lastBloodSight)
                {
                    _lastInvisibility = invisibility;
                    _lastPuddle = puddle;
                    _lastBloodSight = bloodSight;
                    _channel.RaiseCooldowns(new HudCooldowns(invisibility, puddle, bloodSight));
                }

                return; // single player
            }
        }

        public void Dispose()
        {
        }

        private static float Fraction(float remaining, float total)
        {
            if (total <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(remaining / total);
        }
    }
}
