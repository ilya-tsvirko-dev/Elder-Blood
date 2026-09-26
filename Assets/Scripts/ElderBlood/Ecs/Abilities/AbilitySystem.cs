using ElderBlood.Configs;
using ElderBlood.Ecs.Audio;
using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Character;
using ElderBlood.Ecs.Guard;
using ElderBlood.Ecs.Health;
using ElderBlood.Ecs.Movement;
using ElderBlood.Services;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Abilities
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AbilitySystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<AbilitiesComponent> _abilitiesStash;
        private Stash<BloodComponent> _bloodStash;
        private Stash<MoveComponent> _moveStash;
        private Stash<HealthShaderComponent> _shaderStash;
        private Stash<InvisibilityActiveComponent> _invisibilityStash;
        private Stash<BloodPuddleActiveComponent> _puddleStash;
        private Stash<BloodSightActiveComponent> _bloodSightStash;
        private Stash<CharacterAnimationComponent> _animStash;

        private Filter _guardFilter;
        private Stash<GuardPerceptionComponent> _perceptionStash;

        private Event<AbilityCastEvent> _castEvent;
        private Event<SfxEvent> _sfxEvent;

        private readonly AbilityConfig _config;

        public AbilitySystem(MainConfig config)
        {
            _config = config.AbilityConfig;
        }

        public void OnAwake()
        {
            _filter = World.Filter.With<AbilitiesComponent>().With<BloodComponent>()
                .With<MoveComponent>().With<HealthShaderComponent>().Build();
            _abilitiesStash = World.GetStash<AbilitiesComponent>();
            _bloodStash = World.GetStash<BloodComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _shaderStash = World.GetStash<HealthShaderComponent>();
            _invisibilityStash = World.GetStash<InvisibilityActiveComponent>();
            _puddleStash = World.GetStash<BloodPuddleActiveComponent>();
            _bloodSightStash = World.GetStash<BloodSightActiveComponent>();
            _animStash = World.GetStash<CharacterAnimationComponent>();

            _guardFilter = World.Filter.With<GuardPerceptionComponent>().Build();
            _perceptionStash = World.GetStash<GuardPerceptionComponent>();

            _castEvent = World.GetEvent<AbilityCastEvent>();
            _sfxEvent = World.GetEvent<SfxEvent>();
            _castEvent.Subscribe(OnCast);
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var abilities = ref _abilitiesStash.Get(entity);
                if (abilities.invisibilityCooldown > 0f)
                {
                    abilities.invisibilityCooldown -= deltaTime;
                }

                if (abilities.puddleCooldown > 0f)
                {
                    abilities.puddleCooldown -= deltaTime;
                }

                if (abilities.bloodSightCooldown > 0f)
                {
                    abilities.bloodSightCooldown -= deltaTime;
                }
            }
        }

        public void Dispose()
        {
        }

        private void OnCast(FastList<AbilityCastEvent> events)
        {
            foreach (var cast in events)
            {
                foreach (var entity in _filter)
                {
                    switch (cast.type)
                    {
                        case AbilityType.Invisibility:
                            TryInvisibility(entity);
                            break;
                        case AbilityType.BloodPuddle:
                            TryBloodPuddle(entity);
                            break;
                        case AbilityType.BloodSight:
                            TryBloodSight(entity);
                            break;
                    }
                }
            }
        }

        private void TryInvisibility(Entity entity)
        {
            ref var abilities = ref _abilitiesStash.Get(entity);
            var config = _config;

            if (abilities.invisibilityCooldown > 0f || _invisibilityStash.Has(entity))
            {
                GameLogger.Ability($"[Ability] Invisibility blocked: cooldown {abilities.invisibilityCooldown:0.#}s / active {_invisibilityStash.Has(entity)}");
                return;
            }

            ref var blood = ref _bloodStash.Get(entity);
            if (blood.current < config.invisibilityCost)
            {
                GameLogger.Ability($"[Ability] Invisibility denied: need {config.invisibilityCost} blood, have {blood.current:0.#}");
                return;
            }

            blood.current -= config.invisibilityCost;
            abilities.invisibilityCooldown = config.invisibilityCooldown;

            _invisibilityStash.Set(entity, new InvisibilityActiveComponent { timer = config.invisibilityDuration });
            _moveStash.Get(entity).speedMultiplier = config.invisibilitySpeedMultiplier;
            _shaderStash.Get(entity).isFaded = true;

            _sfxEvent.NextFrame(new SfxEvent { type = SfxType.AbilityCast });
            GameLogger.Ability($"[Ability] Invisibility cast: -{config.invisibilityCost} blood → {blood.current:0.#} left");
        }

        private void TryBloodPuddle(Entity entity)
        {
            // Toggle: pressing the key again while in the puddle stands up (WakeUp).
            if (_puddleStash.Has(entity))
            {
                _puddleStash.Remove(entity);
                _animStash.Get(entity).type = CharacterAnimationType.WakeUp;
                GameLogger.Ability("[Ability] BloodPuddle: wake up (toggle off)");
                return;
            }

            ref var abilities = ref _abilitiesStash.Get(entity);
            var config = _config;

            if (abilities.puddleCooldown > 0f)
            {
                GameLogger.Ability($"[Ability] BloodPuddle blocked: cooldown {abilities.puddleCooldown:0.#}s");
                return;
            }

            ref var blood = ref _bloodStash.Get(entity);
            if (blood.current < config.puddleCost)
            {
                GameLogger.Ability($"[Ability] BloodPuddle denied: need {config.puddleCost} blood, have {blood.current:0.#}");
                return;
            }

            // Does not work if any guard is currently aware of the player.
            if (AnyGuardAware())
            {
                GameLogger.Ability("[Ability] BloodPuddle denied: a guard is aware of the player");
                return;
            }

            blood.current -= config.puddleCost;
            abilities.puddleCooldown = config.puddleCooldown;

            ref var move = ref _moveStash.Get(entity);
            var origin = move.rigidbody.position;
            move.isAction = true;

            _puddleStash.Set(entity, new BloodPuddleActiveComponent
            {
                origin = origin,
                radius = config.puddleRadius
            });
            _animStash.Get(entity).type = CharacterAnimationType.Puddle;

            _sfxEvent.NextFrame(new SfxEvent { type = SfxType.AbilityCast });
            GameLogger.Ability($"[Ability] BloodPuddle cast: -{config.puddleCost} blood → {blood.current:0.#} left");
        }

        private void TryBloodSight(Entity entity)
        {
            ref var abilities = ref _abilitiesStash.Get(entity);
            var config = _config;

            if (abilities.bloodSightCooldown > 0f || _bloodSightStash.Has(entity))
            {
                GameLogger.Ability($"[Ability] BloodSight blocked: cooldown {abilities.bloodSightCooldown:0.#}s / active {_bloodSightStash.Has(entity)}");
                return;
            }

            ref var blood = ref _bloodStash.Get(entity);
            if (blood.current < config.bloodSightCost)
            {
                GameLogger.Ability($"[Ability] BloodSight denied: need {config.bloodSightCost} blood, have {blood.current:0.#}");
                return;
            }

            blood.current -= config.bloodSightCost;
            abilities.bloodSightCooldown = config.bloodSightCooldown;

            _bloodSightStash.Set(entity, new BloodSightActiveComponent
            {
                timer = config.bloodSightDuration,
                radius = config.bloodSightRadius,
                color = config.bloodSightColor
            });

            _sfxEvent.NextFrame(new SfxEvent { type = SfxType.AbilityCast });
            GameLogger.Ability($"[Ability] BloodSight cast: -{config.bloodSightCost} blood → {blood.current:0.#} left");
        }

        private bool AnyGuardAware()
        {
            foreach (var guard in _guardFilter)
            {
                if (_perceptionStash.Get(guard).state != PerceptionState.Unaware)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
