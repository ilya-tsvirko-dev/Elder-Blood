using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Level
{
    // Floor objective (v1 = reach the exit). On exit reached → advance the run; on player death → restart it.
    // Bridges ECS to the MonoBehaviour LevelManager (injected).
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ObjectiveSystem : ISystem
    {
        public World World { get; set; }

        private Filter _playerFilter;
        private Filter _exitFilter;
        private Stash<MoveComponent> _moveStash;
        private Stash<ExitComponent> _exitStash;

        private Event<PlayerDeathEvent> _deathEvent;

        private readonly LevelManager _levelManager;

        public ObjectiveSystem(LevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public void OnAwake()
        {
            _playerFilter = World.Filter.With<MoveComponent>().With<BloodComponent>().Build();
            _exitFilter = World.Filter.With<ExitComponent>().Build();
            _moveStash = World.GetStash<MoveComponent>();
            _exitStash = World.GetStash<ExitComponent>();

            _deathEvent = World.GetEvent<PlayerDeathEvent>();
            _deathEvent.Subscribe(OnPlayerDeath);
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var player in _playerFilter)
            {
                Vector2 playerPos = _moveStash.Get(player).rigidbody.position;

                foreach (var exit in _exitFilter)
                {
                    ref var exitData = ref _exitStash.Get(exit);
                    var sqr = ((Vector2)exitData.transform.position - playerPos).sqrMagnitude;
                    if (sqr <= exitData.reachRadius * exitData.reachRadius)
                    {
                        Debug.Log("[Run] Exit reached → advancing floor");
                        _levelManager.AdvanceFloor();
                        return;
                    }
                }
            }
        }

        public void Dispose()
        {
        }

        private void OnPlayerDeath(FastList<PlayerDeathEvent> events)
        {
            foreach (var _ in events)
            {
                Debug.Log("[Run] Player died → restarting run");
                _levelManager.RestartRun();
                return;
            }
        }
    }
}
