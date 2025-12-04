using ElderBlood.Character;
using ElderBlood.Guard.Animation;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace ElderBlood.Guard.GuardBehavior
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

        public void OnAwake()
        {
            _filter = World.Filter.With<GuardBehaviorComponent>().With<MoveComponent>().With<GuardAnimationComponent>()
                .With<MoveNavMeshComponent>().Build();
            _stash = World.GetStash<GuardBehaviorComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _animationStash = World.GetStash<GuardAnimationComponent>();
            _navigateStash = World.GetStash<MoveNavMeshComponent>();
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

                if (behaviorComponent.targetTransform == null)
                {
                    moveComponent.moveVector = Vector3.zero;
                    animationComponent.moveType = MoveAnimationType.Stop;
                    continue;
                }

                var canDoPath = NavMesh.CalculatePath(behaviorComponent.transform.position, behaviorComponent.targetTransform.position,
                    NavMesh.AllAreas,
                    navigationComponent.navMeshPath);
                
                if (!canDoPath)
                    continue;
                
                for (int i = 0; i < navigationComponent.navMeshPath.corners.Length - 1; i++)
                    Debug.DrawLine(navigationComponent.navMeshPath.corners[i], navigationComponent.navMeshPath.corners[i + 1], Color.red);
                
                var vector = navigationComponent.navMeshPath.corners[1] - navigationComponent.navMeshPath.corners[0];
                var magnitude = (behaviorComponent.targetTransform.position - behaviorComponent.transform.position)
                    .magnitude;
                var normalizeVector = vector.normalized;

                if (magnitude > behaviorComponent.minMagnitude)
                {
                    moveComponent.moveVector = normalizeVector;
                    animationComponent.moveVector = normalizeVector;
                    animationComponent.moveType = MoveAnimationType.Move;
                    continue;
                }

                moveComponent.moveVector = Vector3.zero;
                animationComponent.moveType = MoveAnimationType.Stop;
                animationComponent.type = GuardAnimationType.Attack;
            }
        }

        public void Dispose()
        {
        }
    }
}