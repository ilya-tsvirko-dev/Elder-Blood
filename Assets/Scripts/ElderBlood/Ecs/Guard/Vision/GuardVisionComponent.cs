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
    public struct GuardVisionComponent : IComponent
    {
        public float viewRadius;
        [Range(0f, 360f)] public float viewAngle;
        public Vector2 eyeOffset;
        public LayerMask obstacleMask;
        public float fillSpeed;
        public float decaySpeed;

        [Title("Runtime State")]
        [ReadOnly] public Vector2 facing;
    }
}
