using ElderBlood.Character;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Guard.Animation
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GuardAnimationComponent : IComponent 
    {
        public Animator animator;
        public GuardAnimationType type;
        public MoveAnimationType moveType;
        public Vector2 moveVector;
        public float animationTime;
    }
}