using ElderBlood.Configs;
using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Character;
using ElderBlood.Ecs.Guard;
using ElderBlood.Ecs.Health;
using ElderBlood.Ecs.Movement;
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
                return;
            }

            ref var blood = ref _bloodStash.Get(entity);
            if (blood.current < config.invisibilityCost)
            {
                return;
            }

            blood.current -= config.invisibilityCost;
            abilities.invisibilityCooldown = config.invisibilityCooldown;

            _invisibilityStash.Set(entity, new InvisibilityActiveComponent { timer = config.invisibilityDuration });
            _moveStash.Get(entity).speedMultiplier = config.invisibilitySpeedMultiplier;
            _shaderStash.Get(entity).isFaded = true;
        }

        private void TryBloodPuddle(Entity entity)
        {
            // Toggle: pressing the key again while in the puddle stands up (WakeUp).
            if (_puddleStash.Has(entity))
            {
                _puddleStash.Remove(entity);
                _animStash.Get(entity).type = CharacterAnimationType.WakeUp;
                return;
            }

            ref var abilities = ref _abilitiesStash.Get(entity);
            var config = _config;

            if (abilities.puddleCooldown > 0f)
            {
                return;
            }

            ref var blood = ref _bloodStash.Get(entity);
            if (blood.current < config.puddleCost)
            {
                return;
            }

            // Does not work if any guard is currently aware of the player.
            if (AnyGuardAware())
            {
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
        }

        private void TryBloodSight(Entity entity)
        {
            ref var abilities = ref _abilitiesStash.Get(entity);
            var config = _config;

            if (abilities.bloodSightCooldown > 0f || _bloodSightStash.Has(entity))
            {
                return;
            }

            ref var blood = ref _bloodStash.Get(entity);
            if (blood.current < config.bloodSightCost)
            {
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
