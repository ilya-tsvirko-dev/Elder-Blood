using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Character;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Abilities
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BloodPuddleSystem : ISystem
    {
        public World World { get; set; }

        private Filter _playerFilter;
        private Filter _sourceFilter;
        private Stash<BloodPuddleActiveComponent> _puddleStash;
        private Stash<BloodComponent> _bloodStash;
        private Stash<BloodSourceComponent> _sourceStash;
        private Stash<CharacterAnimationComponent> _animStash;

        public void OnAwake()
        {
            _playerFilter = World.Filter.With<BloodPuddleActiveComponent>().With<BloodComponent>().Build();
            _sourceFilter = World.Filter.With<BloodSourceComponent>().Build();
            _puddleStash = World.GetStash<BloodPuddleActiveComponent>();
            _bloodStash = World.GetStash<BloodComponent>();
            _sourceStash = World.GetStash<BloodSourceComponent>();
            _animStash = World.GetStash<CharacterAnimationComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var player in _playerFilter)
            {
                ref var puddle = ref _puddleStash.Get(player);
                ref var blood = ref _bloodStash.Get(player);
                Vector2 origin = puddle.origin;
                var radiusSqr = puddle.radius * puddle.radius;

                foreach (var sourceEntity in _sourceFilter)
                {
                    ref var source = ref _sourceStash.Get(sourceEntity);
                    if (source.amount <= 0f)
                    {
                        continue;
                    }

                    if (((Vector2)source.transform.position - origin).sqrMagnitude > radiusSqr)
                    {
                        continue;
                    }

                    var transfer = Mathf.Min(source.amount, blood.capacity - blood.current);
                    blood.current += transfer;
                    source.amount = 0f;
                    UnityEngine.Object.Destroy(source.transform.gameObject);

                    _puddleStash.Remove(player);
                    _animStash.Get(player).type = CharacterAnimationType.WakeUp;
                    break;
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
