using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct MoveComponent : IComponent
    {
        public Rigidbody2D rigidbody;
        public float speed;
        public float runSpeed;

        [Title("Runtime State")]
        [ReadOnly] public Vector2 moveVector;
        [ReadOnly] public bool isRun;
        [ReadOnly] public bool isAction;
    }
}
