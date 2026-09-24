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
        // Ability icons are 32px tall. Quantising readiness to that grid keeps the fill edge
        // on a whole pixel row and collapses per-frame churn into 32 events per cooldown.
        private const float ReadinessSteps = 32f;

        public World World { get; set; }

        private Filter _filter;
        private Stash<BloodComponent> _bloodStash;
        private Stash<AbilitiesComponent> _abilitiesStash;

        private readonly HudChannel _channel;
        private readonly AbilityConfig _config;

        private float _lastCurrent = float.NaN;
        private float _lastCapacity = float.NaN;
        private HudAbilities _lastAbilities;
        private bool _hasAbilities;

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
                var state = new HudAbilities(
                    State(abilities.invisibilityCooldown, _config.invisibilityCooldown, blood.current, _config.invisibilityCost),
                    State(abilities.puddleCooldown, _config.puddleCooldown, blood.current, _config.puddleCost),
                    State(abilities.bloodSightCooldown, _config.bloodSightCooldown, blood.current, _config.bloodSightCost));

                if (!_hasAbilities || !state.Equals(_lastAbilities))
                {
                    _hasAbilities = true;
                    _lastAbilities = state;
                    _channel.RaiseAbilities(state);
                }

                return; // single player
            }
        }

        public void Dispose()
        {
        }

        private static HudAbilityState State(float remaining, float total, float blood, float cost)
        {
            return new HudAbilityState(Readiness(remaining, total), blood >= cost);
        }

        private static float Readiness(float remaining, float total)
        {
            if (total <= 0f)
            {
                return 1f;
            }

            var ready = 1f - Mathf.Clamp01(remaining / total);

            // Floor rather than round: never show the ability as ready a step early.
            return Mathf.Floor(ready * ReadinessSteps) / ReadinessSteps;
        }
    }
}
