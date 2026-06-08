using System;
using ElderBlood.Common;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Character
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CharacterAnimationSystem : ISystem 
    {
        public World World { get; set;}
    
        private Filter _filter;

        private Stash<CharacterAnimationComponent> _animationStash;
        private Stash<MoveComponent> _moveStash;

        public void OnAwake()
        {
            _filter = World.Filter.With<CharacterAnimationComponent>().With<MoveComponent>().Build();
            _animationStash = World.GetStash<CharacterAnimationComponent>();
            _moveStash = World.GetStash<MoveComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                ref var animationComponent = ref _animationStash.Get(entity);
                ref var moveComponent = ref _moveStash.Get(entity);
                var animator = animationComponent.animator;
                var moveVector = animationComponent.moveVector;

                animator.SetFloat(AnimatorParams.XDirection, moveVector.x);
                animator.SetFloat(AnimatorParams.YDirection, moveVector.y);

                if (animationComponent.type != CharacterAnimationType.None)
                {
                    var actionType = animationComponent.type;
                    SetActionAnimation(animator, actionType);
                    animationComponent.type = CharacterAnimationType.None;

                    // Sit is held until key release (cleared by WakeUp), so it gets no timer.
                    // One-shot actions lock movement for the clip duration.
                    if (actionType != CharacterAnimationType.Sit)
                    {
                        var duration = GetActionDuration(animator, actionType);
                        if (duration > 0f)
                        {
                            animationComponent.actionTimer = duration;
                        }
                        else
                        {
                            moveComponent.isAction = false;
                        }
                    }
                }

                // While an action is active, count its timer down and skip move animations.
                if (moveComponent.isAction)
                {
                    if (animationComponent.actionTimer > 0f)
                    {
                        animationComponent.actionTimer -= deltaTime;
                        if (animationComponent.actionTimer <= 0f)
                        {
                            animationComponent.actionTimer = 0f;
                            moveComponent.isAction = false;
                        }
                    }

                    continue;
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
                        throw new ArgumentOutOfRangeException(nameof(animationComponent.moveType), animationComponent.moveType, null);
                }
            }
        }

        private void SetActionAnimation(Animator animator, CharacterAnimationType type)
        {
            animator.ResetTrigger(AnimatorParams.Idle);
            animator.ResetTrigger(AnimatorParams.Move);
            animator.ResetTrigger(AnimatorParams.Run);

            var trigger = type switch
            {
                CharacterAnimationType.Sit => AnimatorParams.Sit,
                CharacterAnimationType.Jump => AnimatorParams.Jump,
                CharacterAnimationType.Death => AnimatorParams.Death,
                CharacterAnimationType.Magic => AnimatorParams.Magic,
                CharacterAnimationType.WakeUp => AnimatorParams.WakeUp,
                CharacterAnimationType.Hide => AnimatorParams.Hide,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            animator.SetTrigger(trigger);
        }

        // Action clips are directional (e.g. "JumpDownSide", "JumpLeftSide"); all directions of one
        // action share the same length, so the first clip whose name starts with the action gives the duration.
        private static float GetActionDuration(Animator animator, CharacterAnimationType type)
        {
            var clipPrefix = type switch
            {
                CharacterAnimationType.Sit => AnimatorParams.SitName,
                CharacterAnimationType.Jump => AnimatorParams.JumpName,
                CharacterAnimationType.Death => AnimatorParams.DeathName,
                CharacterAnimationType.Magic => AnimatorParams.MagicName,
                CharacterAnimationType.WakeUp => AnimatorParams.WakeUpName,
                CharacterAnimationType.Hide => AnimatorParams.HideName,
                _ => null
            };

            if (clipPrefix == null)
            {
                return 0f;
            }

            var controller = animator.runtimeAnimatorController;
            if (controller == null)
            {
                return 0f;
            }

            var clips = controller.animationClips;
            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i].name.StartsWith(clipPrefix))
                {
                    return clips[i].length;
                }
            }

            return 0f;
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

        public void Dispose()
        {

        }
    }
}