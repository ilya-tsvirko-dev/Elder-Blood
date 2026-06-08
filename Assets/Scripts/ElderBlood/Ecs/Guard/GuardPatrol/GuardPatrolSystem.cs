using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardPatrolSystem : ISystem
    {
        public World World { get; set; }
        
        private Filter _filter;
        private Stash<GuardBehaviorComponent> _stash;
        private Stash<GuardPatrolComponent> _patrolStash;
        private Stash<GuardAlertComponent> _alertStash;

        public void OnAwake()
        {
            _filter = World.Filter.With<GuardBehaviorComponent>().With<GuardPatrolComponent>()
                .With<GuardAlertComponent>().Build();
            _stash = World.GetStash<GuardBehaviorComponent>();
            _patrolStash = World.GetStash<GuardPatrolComponent>();
            _alertStash = World.GetStash<GuardAlertComponent>();
        }
        
        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var behaviorComponent = ref _stash.Get(entity);
                ref var patrolComponent = ref _patrolStash.Get(entity);
                ref var alertComponent = ref _alertStash.Get(entity);

                DrawDebug(patrolComponent.points, patrolComponent.currentPoint);

                // Patrol advances only in the Patrol state; other states set the destination via GuardAlertSystem.
                if (alertComponent.state != GuardAlertState.Patrol)
                    continue;

                if (patrolComponent.currentPoint == Vector3.zero)
                {
                    patrolComponent.currentIndex = 0;
                    patrolComponent.currentPoint = patrolComponent.points[patrolComponent.currentIndex];
                }

                if (patrolComponent.waitTimer > 0f)
                {
                    patrolComponent.waitTimer -= deltaTime;
                    behaviorComponent.patrolPoint = Vector3.zero;
                    continue;
                }

                Vector2 currentPosition = behaviorComponent.transform.position;

                if (Vector2.Distance(currentPosition, patrolComponent.currentPoint) < 0.1f)
                {
                    patrolComponent.waitTimer = patrolComponent.waitDuration;
                    patrolComponent.currentIndex = GetNextIndex(patrolComponent.points, patrolComponent.currentIndex);
                    patrolComponent.currentPoint = patrolComponent.points[patrolComponent.currentIndex];
                }

                behaviorComponent.patrolPoint = patrolComponent.currentPoint;
            }
        }
        
        public void Dispose()
        {
            
        }

        private static int GetNextIndex(List<Vector3> points, int index)
        {
            var pointsCount = points.Count;
            return index + 1 < pointsCount ? index + 1 : 0;
        }

        private static void DrawDebug(List<Vector3> points, Vector3 currentPoint)
        {
            if (points == null || points.Count == 0)
            {
                return;
            }

            for (var i = 0; i < points.Count; i++)
            {
                var next = (i + 1) % points.Count;
                Debug.DrawLine(points[i], points[next], Color.cyan);
            }

            if (currentPoint != Vector3.zero)
            {
                const float size = 0.15f;
                Debug.DrawLine(currentPoint + Vector3.left * size, currentPoint + Vector3.right * size, Color.yellow);
                Debug.DrawLine(currentPoint + Vector3.up * size, currentPoint + Vector3.down * size, Color.yellow);
            }
        }
    }
}