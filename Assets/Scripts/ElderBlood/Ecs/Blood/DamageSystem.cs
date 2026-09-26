using ElderBlood.Ecs.Audio;
using ElderBlood.Ecs.Character;
using ElderBlood.Ecs.Movement;
using ElderBlood.Services;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

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
        private Stash<ShieldComponent> _shieldStash;
        private Stash<CharacterAnimationComponent> _animationStash;
        private Stash<MoveComponent> _moveStash;

        private Event<DamageEvent> _damageEvent;
        private Event<PlayerDeathEvent> _deathEvent;
        private Event<SfxEvent> _sfxEvent;

        public void OnAwake()
        {
            _filter = World.Filter.With<BloodComponent>().With<CharacterAnimationComponent>()
                .With<MoveComponent>().Build();
            _bloodStash = World.GetStash<BloodComponent>();
            _shieldStash = World.GetStash<ShieldComponent>();
            _animationStash = World.GetStash<CharacterAnimationComponent>();
            _moveStash = World.GetStash<MoveComponent>();

            _damageEvent = World.GetEvent<DamageEvent>();
            _deathEvent = World.GetEvent<PlayerDeathEvent>();
            _sfxEvent = World.GetEvent<SfxEvent>();
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

                    var remaining = damage.amount;

                    // Guard damage is continuous DPS; AudioService keeps the same clip from retriggering.
                    _sfxEvent.NextFrame(new SfxEvent { type = SfxType.Damage });

                    // Shield absorbs first (if present and unlocked).
                    if (_shieldStash.Has(entity))
                    {
                        ref var shield = ref _shieldStash.Get(entity);
                        if (shield.unlocked && shield.current > 0f)
                        {
                            var absorbed = shield.current < remaining ? shield.current : remaining;
                            shield.current -= absorbed;
                            remaining -= absorbed;

                            if (shield.current <= 0f)
                            {
                                GameLogger.Damage("[Damage] Shield broke");
                            }
                        }
                    }

                    if (remaining <= 0f)
                    {
                        continue;
                    }

                    blood.current -= remaining;
                    if (blood.current > 0f)
                    {
                        continue;
                    }

                    blood.current = 0f;
                    _animationStash.Get(entity).type = CharacterAnimationType.Death;
                    _moveStash.Get(entity).isAction = true;
                    _deathEvent.NextFrame(new PlayerDeathEvent());

                    GameLogger.Damage("[Damage] Player died (blood depleted)");
                }
            }
        }
    }
}
