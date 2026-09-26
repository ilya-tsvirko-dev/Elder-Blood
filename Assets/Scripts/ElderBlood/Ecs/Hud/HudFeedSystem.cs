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
        private Stash<ShieldComponent> _shieldStash;
        private Stash<BloodSightActiveComponent> _bloodSightStash;

        private readonly HudChannel _channel;
        private readonly AbilityConfig _config;

        private float _lastCurrent = float.NaN;
        private float _lastCapacity = float.NaN;
        private float _lastShieldCurrent = float.NaN;
        private float _lastShieldMax = float.NaN;
        private bool _lastShieldUnlocked;
        private bool _lastBloodSightActive;
        private float _lastBloodSightReadiness = float.NaN;
        private HudAbilities _lastAbilities;
        private bool _hasAbilities;
        private bool _hadPlayer;

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
            _shieldStash = World.GetStash<ShieldComponent>();
            _bloodSightStash = World.GetStash<BloodSightActiveComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            var hasPlayer = false;

            foreach (var entity in _filter)
            {
                hasPlayer = true;
                _hadPlayer = true;

                ref var blood = ref _bloodStash.Get(entity);
                if (blood.current != _lastCurrent || blood.capacity != _lastCapacity)
                {
                    _lastCurrent = blood.current;
                    _lastCapacity = blood.capacity;
                    _channel.RaiseBlood(blood.current, blood.capacity);
                }

                ref var shield = ref _shieldStash.Get(entity, out var hasShield);
                if (hasShield && (shield.current != _lastShieldCurrent || shield.max != _lastShieldMax ||
                                  shield.unlocked != _lastShieldUnlocked))
                {
                    _lastShieldCurrent = shield.current;
                    _lastShieldMax = shield.max;
                    _lastShieldUnlocked = shield.unlocked;
                    _channel.RaiseShield(shield.current, shield.max, shield.unlocked);
                }

                // Quantised like the ability icons: sight.timer moves every frame, so comparing it raw
                // fires an event (and a full-screen canvas rebuild) on all ~300 frames of one cast.
                ref var sight = ref _bloodSightStash.Get(entity, out var hasSight);
                var sightReadiness = hasSight ? Remaining(sight.timer, _config.bloodSightDuration) : 0f;
                if (hasSight != _lastBloodSightActive || sightReadiness != _lastBloodSightReadiness)
                {
                    _lastBloodSightActive = hasSight;
                    _lastBloodSightReadiness = sightReadiness;
                    _channel.RaiseBloodSight(hasSight, sightReadiness);
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

                break; // single player
            }

            // Death and restart destroy the player entity, so nothing inside the loop runs and every
            // HUD element would keep showing whatever it held. Reset the whole cache in one place —
            // doing it per element is how the vignette and the shield bar each got left on screen.
            if (!hasPlayer && _hadPlayer)
            {
                ResetCache();
            }
        }

        private void ResetCache()
        {
            _hadPlayer = false;
            _lastCurrent = float.NaN;
            _lastCapacity = float.NaN;
            _lastShieldCurrent = float.NaN;
            _lastShieldMax = float.NaN;
            _lastShieldUnlocked = false;
            _lastBloodSightReadiness = float.NaN;
            _hasAbilities = false;

            _channel.RaiseShield(0f, 0f, false);

            if (_lastBloodSightActive)
            {
                _lastBloodSightActive = false;
                _channel.RaiseBloodSight(false, 0f);
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

            // Floor rather than round: never show the ability as ready a step early.
            return Quantise(1f - Mathf.Clamp01(remaining / total));
        }

        // Fraction of the duration still left, for effects that fade out rather than fill up.
        private static float Remaining(float remaining, float total)
        {
            return total <= 0f ? 1f : Quantise(remaining / total);
        }

        private static float Quantise(float value)
        {
            return Mathf.Floor(Mathf.Clamp01(value) * ReadinessSteps) / ReadinessSteps;
        }
    }
}
