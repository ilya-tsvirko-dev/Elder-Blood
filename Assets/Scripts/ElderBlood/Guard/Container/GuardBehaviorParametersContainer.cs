using System;
using UnityEngine;

namespace ElderBlood.Guard.Container
{
    public class GuardBehaviorParametersContainer : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private float _shiftSpeed;
        [SerializeField] private bool _isCheckAnimation;
        [SerializeField] private bool _isRun;
        [SerializeField] private bool _isIdle;
        [Space]
        [SerializeField] private GuardAnimation _guardAnimation;
        [Space]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private bool _isDebug;
        [SerializeField] private int _segments;
        [SerializeField] private float _zPosition;
        [SerializeField] private float _radius;
        [SerializeField] private float _attackRadius;
        
        private Rigidbody2D _rigidbody;  
        private Transform _transform;
        
        public Rigidbody2D Rigidbody2D => _rigidbody;
        public Transform Transform => _transform;
        
        public float Speed => _speed;
        public float ShiftSpeed => _shiftSpeed;
        public bool IsCheckAnimation => _isCheckAnimation;
        
        public GuardAnimation GuardAnimation => _guardAnimation;
        
        public LineRenderer LineRenderer => _lineRenderer;
        public bool Debug => _isDebug;
        public int Segments => _segments;
        public float ZPosition => _zPosition;
        public float Radius => _radius;
        public float AttackRadius => _attackRadius;

        public Action ChangeParameters;

        public void OnAwake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _transform = GetComponent<Transform>();
        }

        private void OnValidate()
        {
            ChangeParameters?.Invoke();
        }
    }
}