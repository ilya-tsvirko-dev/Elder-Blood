using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Character;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace ElderBlood.Ecs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardBehaviorSystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<GuardBehaviorComponent> _stash;
        private Stash<MoveComponent> _moveStash;
        private Stash<MoveNavMeshComponent> _navigateStash;
        private Stash<GuardAnimationComponent> _animationStash;

        private Event<DamageEvent> _damageEvent;

        public void OnAwake()
        {
            _filter = World.Filter.With<GuardBehaviorComponent>().With<MoveComponent>().With<GuardAnimationComponent>()
                .With<MoveNavMeshComponent>().Build();
            _stash = World.GetStash<GuardBehaviorComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _animationStash = World.GetStash<GuardAnimationComponent>();
            _navigateStash = World.GetStash<MoveNavMeshComponent>();

            _damageEvent = World.GetEvent<DamageEvent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var behaviorComponent = ref _stash.Get(entity);
                ref var moveComponent = ref _moveStash.Get(entity);
                ref var animationComponent = ref _animationStash.Get(entity);
                ref var navigationComponent = ref _navigateStash.Get(entity);
                
                animationComponent.type = GuardAnimationType.None;

                if (behaviorComponent.targetTransform == null && behaviorComponent.patrolPoint == Vector3.zero)
                {
                    moveComponent.moveVector = Vector3.zero;
                    animationComponent.moveType = MoveAnimationType.Stop;
                    continue;
                }
                
                var pointToMove = behaviorComponent.targetTransform != null ?
                    behaviorComponent.targetTransform.position :  behaviorComponent.patrolPoint;

                // Attack is checked before pathfinding on purpose: at point-blank range the path collapses
                // to a single corner, and gating the attack behind a path made the guard skip it entirely.
                if (behaviorComponent.targetTransform != null && IsInAttackReach(ref behaviorComponent, pointToMove))
                {
                    moveComponent.moveVector = Vector3.zero;
                    animationComponent.moveType = MoveAnimationType.Stop;
                    animationComponent.type = GuardAnimationType.Attack;
                    _damageEvent.NextFrame(new DamageEvent { amount = behaviorComponent.attackDamage * deltaTime });
                    continue;
                }

                var canDoPath = NavMesh.CalculatePath(behaviorComponent.transform.position, pointToMove,
                    NavMesh.AllAreas,
                    navigationComponent.navMeshPath);

                if (!canDoPath || navigationComponent.navMeshPath.corners.Length < 2)
                {
                    moveComponent.moveVector = Vector3.zero;
                    animationComponent.moveType = MoveAnimationType.Stop;
                    continue;
                }

                for (int i = 0; i < navigationComponent.navMeshPath.corners.Length - 1; i++)
                    Debug.DrawLine(navigationComponent.navMeshPath.corners[i], navigationComponent.navMeshPath.corners[i + 1], Color.red);

                var vector = navigationComponent.navMeshPath.corners[1] - navigationComponent.navMeshPath.corners[0];
                var normalizeVector = vector.normalized;

                moveComponent.moveVector = normalizeVector;
                animationComponent.moveVector = normalizeVector;
                animationComponent.moveType = MoveAnimationType.Move;
            }
        }

        // Reach is the gap between the two bodies, measured surface to surface — the same in every
        // direction. Falls back to a centre distance only if either collider is missing or disabled.
        private static bool IsInAttackReach(ref GuardBehaviorComponent behavior, Vector3 targetPosition)
        {
            if (behavior.collider != null && behavior.targetCollider != null)
            {
                var gap = behavior.collider.Distance(behavior.targetCollider);
                if (gap.isValid)
                {
                    return gap.distance <= behavior.attackRange;
                }
            }

            return (targetPosition - behavior.transform.position).magnitude <= behavior.attackRange;
        }

        public void Dispose()
        {
        }
    }
}