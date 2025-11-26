using System;
using System.Collections.Generic;
using DefaultNamespace.Configs.Guard;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DefaultNamespace.Configs
{
    [CreateAssetMenu(fileName = "MainConfig", menuName = "Configs/MainConfig", order = 1)]
    public class MainConfig : ScriptableObject
    {
        public GuardConfig GuardConfig;
        
        public event Action OnReset;
        
        [Button]
        public void Reset()
        {
            OnReset?.Invoke();
        }
    }
}