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

        private const string XDirectionID = "XDirection";
        private const string YDirectionID = "YDirection";
    
        public World World { get; set;}
    
        private Filter _filter;
    
        private Stash<CharacterAnimationComponent> _animationStash;

        public void OnAwake() 
        {
            _filter = World.Filter.With<CharacterAnimationComponent>().Build();
            _animationStash = World.GetStash<CharacterAnimationComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                ref var animationComponent = ref _animationStash.Get(entity);
                var animator = animationComponent.animator;
                var moveVector = animationComponent.moveVector;
            
                animator.SetFloat(XDirection, moveVector.x);
                animator.SetFloat(YDirection, moveVector.y);

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

        public void OnAnimationAction(CharacterAnimationType type)
        {
            var trigger = type switch
            {
                CharacterAnimationType.Sit => Sit,
                CharacterAnimationType.Jump => Jump,
                CharacterAnimationType.Death => Death,
                CharacterAnimationType.Magic => Magic,
                CharacterAnimationType.WakeUp => WakeUp,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            //_animator.SetTrigger(trigger);
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