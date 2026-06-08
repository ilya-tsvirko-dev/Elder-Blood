using ElderBlood.Ecs.Character;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Blood
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DamageSystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<BloodComponent> _bloodStash;
        private Stash<CharacterAnimationComponent> _animationStash;
        private Stash<MoveComponent> _moveStash;

        private Event<DamageEvent> _damageEvent;
        private Event<PlayerDeathEvent> _deathEvent;

        public void OnAwake()
        {
            _filter = World.Filter.With<BloodComponent>().With<CharacterAnimationComponent>()
                .With<MoveComponent>().Build();
            _bloodStash = World.GetStash<BloodComponent>();
            _animationStash = World.GetStash<CharacterAnimationComponent>();
            _moveStash = World.GetStash<MoveComponent>();

            _damageEvent = World.GetEvent<DamageEvent>();
            _deathEvent = World.GetEvent<PlayerDeathEvent>();
            _damageEvent.Subscribe(ApplyDamage);
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Dispose()
        {
        }

        private void ApplyDamage(FastList<DamageEvent> damages)
        {
            foreach (var damage in damages)
            {
                foreach (var entity in _filter)
                {
                    ref var blood = ref _bloodStash.Get(entity);
                    if (blood.current <= 0f)
                    {
                        continue;
                    }

                    blood.current -= damage.amount;
                    if (blood.current > 0f)
                    {
                        continue;
                    }

                    blood.current = 0f;
                    _animationStash.Get(entity).type = CharacterAnimationType.Death;
                    _moveStash.Get(entity).isAction = true;
                    _deathEvent.NextFrame(new PlayerDeathEvent());
                }
            }
        }
    }
}
