using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Guard.GuardBehavior
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GuardBehaviorComponent : IComponent 
    {
        public Transform targetTransform;
        public Vector3 patrolPoint;
        public Transform transform;
        public float minMagnitude;
    }
}