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
        public Collider2D collider;

        // Gap between the two bodies, not a centre-to-centre distance: bodies differ in shape, so a
        // radius measured from the centre reaches much further sideways than vertically.
        public float attackRange;
        public float attackDamage;

        [Title("Runtime State")]
        [ReadOnly] public Transform targetTransform;
        [ReadOnly] public Collider2D targetCollider;
        [ReadOnly] public Vector3 patrolPoint;
    }
}