using System;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Character.Animation
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CharacterAnimationSystem : ISystem 
    {
        private static readonly int Move = Animator.StringToHash(MoveTriggerID);
        private static readonly int Run = Animator.StringToHash(RunTriggerID);
        private static readonly int Idle = Animator.StringToHash(IdleTriggerID);
        private static readonly int Sit = Animator.StringToHash(SitTriggerID);
        private static readonly int WakeUp = Animator.StringToHash(WakeUpTriggerID);
        private static readonly int Jump = Animator.StringToHash(JumpTriggerID);
        private static readonly int Death = Animator.StringToHash(DeathTriggerID);
        private static readonly int Magic = Animator.StringToHash(MagicTriggerID);
        private static readonly int Hide = Animator.StringToHash(HideTriggerID);

        private static readonly int XDirection = Animator.StringToHash(XDirectionID);
        private static readonly int YDirection = Animator.StringToHash(YDirectionID);
        
        private const string MoveTriggerID = "Move";
        private const string RunTriggerID = "Run";
        private const string IdleTriggerID = "Idle";
        private const string SitTriggerID = "Sit";
        private const string WakeUpTriggerID = "WakeUp";
        private const string JumpTriggerID = "Jump";
        private const string DeathTriggerID = "Death";
        private const string MagicTriggerID = "Magic";
        private const string HideTriggerID = "Hide";

        private const string XDirectionID = "XDirection";
        private const string YDirectionID = "YDirection";
    
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

                animator.SetFloat(XDirection, moveVector.x);
                animator.SetFloat(YDirection, moveVector.y);

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
            animator.ResetTrigger(Idle);
            animator.ResetTrigger(Move);
            animator.ResetTrigger(Run);

            var trigger = type switch
            {
                CharacterAnimationType.Sit => Sit,
                CharacterAnimationType.Jump => Jump,
                CharacterAnimationType.Death => Death,
                CharacterAnimationType.Magic => Magic,
                CharacterAnimationType.WakeUp => WakeUp,
                CharacterAnimationType.Hide => Hide,
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
                CharacterAnimationType.Sit => SitTriggerID,
                CharacterAnimationType.Jump => JumpTriggerID,
                CharacterAnimationType.Death => DeathTriggerID,
                CharacterAnimationType.Magic => MagicTriggerID,
                CharacterAnimationType.WakeUp => WakeUpTriggerID,
                CharacterAnimationType.Hide => HideTriggerID,
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

        public void Dispose()
        {

        }
    }
}