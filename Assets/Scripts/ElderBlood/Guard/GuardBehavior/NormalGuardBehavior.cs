using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ElderBlood.Character;
using ElderBlood.Character.Animation;
using ElderBlood.Guard.Container;
using UnityEngine;

namespace ElderBlood.Guard.GuardBehavior
{
    public class NormalGuardBehavior : GuardBehaviorAbstract
    {
        private readonly Rigidbody2D _rb;
        private readonly GuardAnimation _guardAnimation;
        private readonly Transform _transform;
        
        private  Transform _targetTransform;
        private Vector2 _moveVector;
        private Vector2 _vector;
        private bool _isAction;
        private bool _isRun;

        private CancellationTokenSource _cts;

        public NormalGuardBehavior(GuardBehaviorParametersContainer container) : base(container)
        {
            _rb = Container.Rigidbody2D;
            _guardAnimation = Container.GuardAnimation;
            _transform = Container.Transform;
        }

        public override void FixedUpdate()
        {
            if (_isAction)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            if (_moveVector == Vector2.zero && !_isRun)
            {
                _rb.linearVelocity = Vector2.zero;
                _guardAnimation.SetMoveAnimation(MoveAnimationType.Stop, _moveVector);
                return;
            }

            _vector = _moveVector * (Time.fixedDeltaTime * (!_isRun ? Container.ShiftSpeed : Container.Speed));
            _rb.linearVelocity = Container.IsCheckAnimation? Vector2.zero : _vector;

            if (!_isRun)
            {
                _guardAnimation.SetMoveAnimation(MoveAnimationType.Run, _moveVector);
                return;
            }

            _guardAnimation.SetMoveAnimation(MoveAnimationType.Move, _moveVector);
        }

        public override void Update()
        {
            
        }

        public override void Enter()
        {
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            
            RunSearching(ct).Forget();
            RunAttackBehavior(ct).Forget();
            
            SetDebugSearching();
            
            Container.ChangeParameters += OnChangeParameters;
        }

        public override void Exit()
        {
            Container.ChangeParameters -= OnChangeParameters;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
        
        private void OnChangeParameters()
        {
            SetDebugSearching();
        }

        private void SetDebugSearching()
        {
            if (Container.Debug)
            {
                Container.LineRenderer.gameObject.SetActive(true);

                Container.LineRenderer.useWorldSpace = false;
                Container.LineRenderer.loop = true;
                Container.LineRenderer.positionCount = Container.Segments;

                Vector3[] points = new Vector3[Container.Segments];
                for (int i = 0; i < Container.Segments; i++)
                {
                    float angle = 2f * Mathf.PI * i / Container.Segments;
                    points[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * Container.Radius;
                    points[i].z = Container.ZPosition;
                }
                
                Container.LineRenderer.SetPositions(points);
            }
            else
            {
                Container.LineRenderer.gameObject.SetActive(false);
            }
        }

        private async UniTaskVoid RunSearching(CancellationToken ct)
        {
            while (ct.IsCancellationRequested == false)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);

                var collider2D = Physics2D.OverlapCircle(_transform.position, Container.Radius, LayerMask.GetMask("Character"));
                
                if (collider2D == null)
                {
                    _moveVector = Vector2.zero;
                    continue;               
                }
                
                _targetTransform = collider2D.transform;
                
                _moveVector = (collider2D.transform.position - _transform.position).normalized;
            }
        }

        private async UniTaskVoid RunAttackBehavior(CancellationToken ct)
        {
            while (ct.IsCancellationRequested == false)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);

                if (_targetTransform != null 
                    && _moveVector != Vector2.zero 
                    && Vector3.Distance(_transform.position, _targetTransform.position) < Container.AttackRadius)
                {
                    RunAction(GuardAction.Attack, ct).Forget();
                }
            }
        }

        private async UniTaskVoid RunAction(GuardAction guardAction, CancellationToken ct)
        {
            if (_isAction)
            {
                return;
            }
            
            _isAction = true;

            switch (guardAction)
            {
                case GuardAction.None:
                    return;
                case GuardAction.Attack:
                    _guardAnimation.SetActionAnimation(CharacterAnimationType.Attack);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(guardAction), guardAction, null);
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.7f), cancellationToken: ct);
            
            _isAction = false;
        }
    }
}