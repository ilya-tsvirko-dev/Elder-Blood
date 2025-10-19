using System;
using ElderBlood.Character;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Guard.Animation
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class GuardAnimationSystem : ISystem
    {
        private static readonly int Move = Animator.StringToHash(MoveTriggerID);
        private static readonly int Run = Animator.StringToHash(RunTriggerID);
        private static readonly int Idle = Animator.StringToHash(IdleTriggerID);
        private static readonly int Attack = Animator.StringToHash(AttackTriggerID);

        private static readonly int XDirection = Animator.StringToHash(XDirectionID);
        private static readonly int YDirection = Animator.StringToHash(YDirectionID);

        private const string MoveTriggerID = "Move";
        private const string RunTriggerID = "Run";
        private const string IdleTriggerID = "Idle";
        private const string AttackTriggerID = "Attack";

        private const string XDirectionID = "XDirection";
        private const string YDirectionID = "YDirection";

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
                    

                animator.SetFloat(XDirection, moveVector.x);
                animator.SetFloat(YDirection, moveVector.y);

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
            animationComponent.animator.SetTrigger(Attack);
        }

        private void SetIdleAnimation(Animator animator)
        {
            animator.ResetTrigger(Move);
            animator.ResetTrigger(Run);
            animator.SetTrigger(Idle);
        }

        private void SetRunAnimation(Animator animator)
        {
            animator.ResetTrigger(Move);
            animator.ResetTrigger(Idle);
            animator.SetTrigger(Run);
        }

        private void SetInputHandlerAnimation(Animator animator)
        {
            animator.ResetTrigger(Idle);
            animator.ResetTrigger(Run);
            animator.SetTrigger(Move);
        }
    }
}