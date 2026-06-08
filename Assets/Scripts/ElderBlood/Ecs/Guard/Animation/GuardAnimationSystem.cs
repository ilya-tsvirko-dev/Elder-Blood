using System;
using ElderBlood.Common;
using ElderBlood.Ecs.Character;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class GuardAnimationSystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<GuardAnimationComponent> _stash;

        public void OnAwake()
        {
            _filter = World.Filter.With<GuardAnimationComponent>().Build();
            _stash = World.GetStash<GuardAnimationComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var animationComponent = ref _stash.Get(entity);

                var animator = animationComponent.animator;
                var moveVector = animationComponent.moveVector;

                if (animationComponent.animationTime > 0)
                {
                    animationComponent.animationTime -= deltaTime;
                    continue;
                }
                    

                animator.SetFloat(AnimatorParams.XDirection, moveVector.x);
                animator.SetFloat(AnimatorParams.YDirection, moveVector.y);

                switch (animationComponent.type)
                {
                    case GuardAnimationType.None:
                        break;
                    case GuardAnimationType.Attack:
                        SetAttackAnimation(ref animationComponent);
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(animationComponent.type), animationComponent.type,
                            null);
                }

                switch (animationComponent.moveType)
                {
                    case MoveAnimationType.Run:
                        SetRunAnimation(animator);
                        break;
                    case MoveAnimationType.Move:
                        SetInputHandlerAnimation(animator);
                        break;
                    case MoveAnimationType.Stop:
                        SetIdleAnimation(animator);
                        break;
                    case MoveAnimationType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(animationComponent.moveType),
                            animationComponent.moveType, null);
                }
            }
        }

        public void Dispose()
        {
        }

        private void SetAttackAnimation(ref GuardAnimationComponent animationComponent)
        {
            animationComponent.animationTime = 1f;
            animationComponent.animator.SetTrigger(AnimatorParams.Attack);
        }

        private void SetIdleAnimation(Animator animator)
        {
            animator.ResetTrigger(AnimatorParams.Move);
            animator.ResetTrigger(AnimatorParams.Run);
            animator.SetTrigger(AnimatorParams.Idle);
        }

        private void SetRunAnimation(Animator animator)
        {
            animator.ResetTrigger(AnimatorParams.Move);
            animator.ResetTrigger(AnimatorParams.Idle);
            animator.SetTrigger(AnimatorParams.Run);
        }

        private void SetInputHandlerAnimation(Animator animator)
        {
            animator.ResetTrigger(AnimatorParams.Idle);
            animator.ResetTrigger(AnimatorParams.Run);
            animator.SetTrigger(AnimatorParams.Move);
        }
    }
}