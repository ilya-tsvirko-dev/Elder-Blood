using System;
using ElderBlood.Guard.Container;
using ElderBlood.Guard.GuardBehavior;
using UnityEngine;

namespace ElderBlood.Guard
{
    public class Guard : MonoBehaviour
    {
        [SerializeField] private GuardBehaviorParametersContainer _container;
        
        private GuardBehaviorAbstract _currentGuardBehavior;
        
        private void Awake()
        {
            _container.OnAwake();
        }

        private void Start()
        {
            _currentGuardBehavior = new NormalGuardBehavior(_container);
            _currentGuardBehavior.Enter();
        }

        private void OnDestroy()
        {
            _currentGuardBehavior.Exit();
        }

        private void Update()
        {
            _currentGuardBehavior?.Update();
        }

        private void FixedUpdate()
        {
            _currentGuardBehavior?.FixedUpdate();
        }
    }
}