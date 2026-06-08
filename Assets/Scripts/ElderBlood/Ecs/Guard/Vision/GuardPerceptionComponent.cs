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
    public struct GuardPerceptionComponent : IComponent
    {
        [Title("Runtime State")]
        [ReadOnly] public float awareness;
        [ReadOnly] public PerceptionState state;
        [ReadOnly] public Vector3 lastSeenPosition;
    }
}
