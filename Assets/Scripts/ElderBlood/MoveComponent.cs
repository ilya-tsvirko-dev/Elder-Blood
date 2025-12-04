using Scellecs.Morpeh;
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
        public Vector2 moveVector;
        public bool isRun;
        public float speed;
        public float runSpeed;
        public bool isAction;
    }
}
