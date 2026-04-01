using System.Collections.Generic;
using ElderBlood.Guard.GuardBehavior;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Guard.GuardPatrol
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
        
        public void OnAwake()
        {
            _filter = World.Filter.With<GuardBehaviorComponent>().With<GuardPatrolComponent>().Build();
            _stash = World.GetStash<GuardBehaviorComponent>();
            _patrolStash = World.GetStash<GuardPatrolComponent>();
        }
        
        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var behaviorComponent = ref _stash.Get(entity);
                ref var patrolComponent = ref _patrolStash.Get(entity);

                if (behaviorComponent.targetTransform != null)
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
    }
}