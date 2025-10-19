using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Character.Animation
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct CharacterAnimationComponent : IComponent 
    {
        public Animator animator;
        public CharacterAnimationType type;
        public MoveAnimationType moveType;
        public Vector2 moveVector;
    }
}