using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Abilities
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BloodSightSystem : ISystem
    {
        public World World { get; set; }

        private Filter _playerFilter;
        private Filter _sourceFilter;
        private Stash<BloodSightActiveComponent> _sightStash;
        private Stash<MoveComponent> _moveStash;
        private Stash<BloodSourceComponent> _sourceStash;

        public void OnAwake()
        {
            _playerFilter = World.Filter.With<BloodSightActiveComponent>().With<MoveComponent>().Build();
            _sourceFilter = World.Filter.With<BloodSourceComponent>().Build();
            _sightStash = World.GetStash<BloodSightActiveComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _sourceStash = World.GetStash<BloodSourceComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var player in _playerFilter)
            {
                ref var sight = ref _sightStash.Get(player);
                sight.timer -= deltaTime;

                if (sight.timer <= 0f)
                {
                    foreach (var sourceEntity in _sourceFilter)
                    {
                        ref var source = ref _sourceStash.Get(sourceEntity);
                        if (source.highlighted)
                        {
                            Unhighlight(ref source);
                        }
                    }

                    _sightStash.Remove(player);
                    continue;
                }

                Vector2 playerPos = _moveStash.Get(player).rigidbody.position;
                var radiusSqr = sight.radius * sight.radius;

                foreach (var sourceEntity in _sourceFilter)
                {
                    ref var source = ref _sourceStash.Get(sourceEntity);
                    if (source.renderer == null)
                    {
                        continue;
                    }

                    var inRadius = ((Vector2)source.transform.position - playerPos).sqrMagnitude <= radiusSqr;

                    if (inRadius && !source.highlighted)
                    {
                        source.baseColor = source.renderer.color;
                        source.renderer.color = sight.color;
                        source.highlighted = true;
                    }
                    else if (!inRadius && source.highlighted)
                    {
                        Unhighlight(ref source);
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        private static void Unhighlight(ref BloodSourceComponent source)
        {
            if (source.renderer != null)
            {
                source.renderer.color = source.baseColor;
            }

            source.highlighted = false;
        }
    }
}
