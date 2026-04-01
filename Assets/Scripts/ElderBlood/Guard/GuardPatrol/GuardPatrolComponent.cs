using System.Collections.Generic;
using Scellecs.Morpeh;
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
        public List<Vector3> points;
        public Vector3 currentPoint;
        public int currentIndex;
        public float waitDuration;
        public float waitTimer;
    }
}