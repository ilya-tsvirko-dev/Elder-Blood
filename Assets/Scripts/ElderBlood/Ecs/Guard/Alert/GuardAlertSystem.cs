using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardAlertSystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<GuardAlertComponent> _alertStash;
        private Stash<GuardPerceptionComponent> _perceptionStash;
        private Stash<GuardBehaviorComponent> _behaviorStash;
        private Stash<GuardVisionComponent> _visionStash;
        private Stash<GuardPatrolComponent> _patrolStash;

        public void OnAwake()
        {
            _filter = World.Filter.With<GuardAlertComponent>().With<GuardPerceptionComponent>()
                .With<GuardBehaviorComponent>().With<GuardVisionComponent>().With<GuardPatrolComponent>().Build();
            _alertStash = World.GetStash<GuardAlertComponent>();
            _perceptionStash = World.GetStash<GuardPerceptionComponent>();
            _behaviorStash = World.GetStash<GuardBehaviorComponent>();
            _visionStash = World.GetStash<GuardVisionComponent>();
            _patrolStash = World.GetStash<GuardPatrolComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var alert = ref _alertStash.Get(entity);
                ref var perception = ref _perceptionStash.Get(entity);
                ref var behavior = ref _behaviorStash.Get(entity);
                ref var vision = ref _visionStash.Get(entity);
                ref var patrol = ref _patrolStash.Get(entity);

                var guardPos = behavior.transform.position;

                switch (alert.state)
                {
                    case GuardAlertState.Patrol:
                        if (perception.state == PerceptionState.Alert) alert.state = GuardAlertState.Chase;
                        else if (perception.state == PerceptionState.Suspicious) alert.state = GuardAlertState.Suspicious;
                        break;

                    case GuardAlertState.Suspicious:
                        if (perception.state == PerceptionState.Alert) alert.state = GuardAlertState.Chase;
                        else if (perception.state == PerceptionState.Unaware) alert.state = GuardAlertState.Patrol;
                        break;

                    case GuardAlertState.Chase:
                        if (perception.state != PerceptionState.Alert)
                        {
                            alert.searchPoint = perception.lastSeenPosition;
                            alert.searchTimer = alert.searchDuration;
                            alert.state = GuardAlertState.Search;
                        }
                        break;

                    case GuardAlertState.Search:
                        if (perception.state == PerceptionState.Alert)
                        {
                            alert.state = GuardAlertState.Chase;
                        }
                        else
                        {
                            alert.searchTimer -= deltaTime;
                            if (alert.searchTimer <= 0f) alert.state = GuardAlertState.Return;
                        }
                        break;

                    case GuardAlertState.Return:
                        if (perception.state == PerceptionState.Alert) alert.state = GuardAlertState.Chase;
                        else if (perception.state == PerceptionState.Suspicious) alert.state = GuardAlertState.Suspicious;
                        else if (Vector3.Distance(guardPos, patrol.currentPoint) <= alert.returnReachDistance)
                            alert.state = GuardAlertState.Patrol;
                        break;
                }

                // Destination per state. targetTransform (chase) is owned by GuardVisionSystem;
                // patrolPoint in Patrol is owned by GuardPatrolSystem.
                switch (alert.state)
                {
                    case GuardAlertState.Suspicious:
                        behavior.patrolPoint = Vector3.zero;
                        FaceTowards(ref vision, guardPos, perception.lastSeenPosition);
                        break;
                    case GuardAlertState.Search:
                        behavior.patrolPoint = alert.searchPoint;
                        break;
                    case GuardAlertState.Return:
                        behavior.patrolPoint = patrol.currentPoint;
                        break;
                }
            }
        }

        public void Dispose()
        {
        }

        private static void FaceTowards(ref GuardVisionComponent vision, Vector3 from, Vector3 to)
        {
            var dir = (Vector2)(to - from);
            if (dir.sqrMagnitude > 0.0001f)
            {
                vision.facing = dir.normalized;
            }
        }
    }
}
