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
    public struct GuardAlertComponent : IComponent
    {
        public float searchDuration;
        public float returnReachDistance;

        [Title("Runtime State")]
        [ReadOnly] public GuardAlertState state;
        [ReadOnly] public float searchTimer;
        [ReadOnly] public Vector3 searchPoint;
    }
}
