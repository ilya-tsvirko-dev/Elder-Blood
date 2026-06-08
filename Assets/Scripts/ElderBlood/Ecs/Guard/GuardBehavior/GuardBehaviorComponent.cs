using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GuardBehaviorComponent : IComponent
    {
        public Transform transform;
        public float minMagnitude;
        public float attackDamage;

        [Title("Runtime State")]
        [ReadOnly] public Transform targetTransform;
        [ReadOnly] public Vector3 patrolPoint;
    }
}