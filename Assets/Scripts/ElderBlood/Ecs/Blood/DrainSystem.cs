using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Blood
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class DrainSystem : ISystem
    {
        public World World { get; set; }

        private Filter _playerFilter;
        private Filter _sourceFilter;
        private Stash<BloodComponent> _bloodStash;
        private Stash<MoveComponent> _moveStash;
        private Stash<BloodSourceComponent> _sourceStash;

        private Event<DrainEvent> _drainEvent;
        private Event<SourceDrainedEvent> _drainedEvent;

        public void OnAwake()
        {
            _playerFilter = World.Filter.With<BloodComponent>().With<MoveComponent>().Build();
            _sourceFilter = World.Filter.With<BloodSourceComponent>().Build();
            _bloodStash = World.GetStash<BloodComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _sourceStash = World.GetStash<BloodSourceComponent>();

            _drainEvent = World.GetEvent<DrainEvent>();
            _drainedEvent = World.GetEvent<SourceDrainedEvent>();
            _drainEvent.Subscribe(OnDrain);
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Dispose()
        {
        }

        private void OnDrain(FastList<DrainEvent> events)
        {
            foreach (var _ in events)
            {
                foreach (var player in _playerFilter)
                {
                    ref var blood = ref _bloodStash.Get(player);
                    if (blood.current >= blood.capacity)
                    {
                        Debug.Log($"[Drain] Blocked: pool full ({blood.current:0.#}/{blood.capacity:0.#})");
                        continue; // pool full → drain blocked
                    }

                    ref var move = ref _moveStash.Get(player);
                    Vector2 playerPos = move.rigidbody.position;

                    Entity nearest = default;
                    var found = false;
                    var nearestSqr = blood.drainRadius * blood.drainRadius;

                    foreach (var sourceEntity in _sourceFilter)
                    {
                        ref var source = ref _sourceStash.Get(sourceEntity);
                        if (source.amount <= 0f || source.state == SourceState.Awake)
                        {
                            continue; // depleted, or woken (no longer easy prey)
                        }

                        var sqr = ((Vector2)source.transform.position - playerPos).sqrMagnitude;
                        if (sqr <= nearestSqr)
                        {
                            nearestSqr = sqr;
                            nearest = sourceEntity;
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        Debug.Log($"[Drain] No source within radius {blood.drainRadius:0.#}");
                        continue;
                    }

                    ref var nearestSource = ref _sourceStash.Get(nearest);
                    var transfer = Mathf.Min(nearestSource.amount, blood.capacity - blood.current);
                    blood.current += transfer;
                    nearestSource.amount -= transfer;

                    _drainedEvent.NextFrame(new SourceDrainedEvent { amount = transfer });

                    Debug.Log($"[Drain] +{transfer:0.#} blood → {blood.current:0.#}/{blood.capacity:0.#}. Source left {nearestSource.amount:0.#}");
                }
            }
        }
    }
}
