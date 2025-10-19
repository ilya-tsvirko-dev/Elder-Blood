using System;
using ElderBlood.Character;
using ElderBlood.Character.Animation;
using UnityEngine;

namespace ElderBlood.Guard
{
    public class GuardAnimation : MonoBehaviour
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
        
        [SerializeField] private Animator _animator;
        
        private Vector2 _currentMoveVector;
        

        public void SetMoveAnimation(MoveAnimationType type, Vector2 vector)
        {
            switch (type)
            {
                case MoveAnimationType.Run:
                    SetRunAnimation(vector);
                    break;
                case MoveAnimationType.Move:
                    SetMoveAnimation(vector);
                    break;
                case MoveAnimationType.Stop:
                    SetIdleAnimation();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void SetActionAnimation(CharacterAnimationType type)
        {
            switch (type)
            {
                case CharacterAnimationType.Attack:
                    _animator.SetTrigger(Attack);
                    break;
            }
        }

        private void SetIdleAnimation()
        {
            _animator.SetFloat(XDirection, _currentMoveVector.x);
            _animator.SetFloat(YDirection, _currentMoveVector.y);

            _animator.ResetTrigger(Move);
            _animator.ResetTrigger(Run);
            _animator.SetTrigger(Idle);
        }
        
        private void SetRunAnimation(Vector2 moveVector)
        {
            _currentMoveVector = moveVector;

            _animator.SetFloat(XDirection, _currentMoveVector.x);
            _animator.SetFloat(YDirection, _currentMoveVector.y);

            _animator.ResetTrigger(Move);
            _animator.ResetTrigger(Idle);
            _animator.SetTrigger(Run);
        }

        private void SetMoveAnimation(Vector2 moveVector)
        {
            _currentMoveVector = moveVector;

            _animator.SetFloat(XDirection, _currentMoveVector.x);
            _animator.SetFloat(YDirection, _currentMoveVector.y);

            _animator.ResetTrigger(Idle);
            _animator.ResetTrigger(Run);
            _animator.SetTrigger(Move);
        }
    }
}