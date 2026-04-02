using System.Collections.Generic;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Guard.GuardPatrol
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    
    public struct GuardPatrolComponent : IComponent
    {
        public float waitDuration;
        public List<Vector3> points;

        [Title("Runtime State")]
        [ReadOnly] public Vector3 currentPoint;
        [ReadOnly] public int currentIndex;
        [ReadOnly] public float waitTimer;
    }
}