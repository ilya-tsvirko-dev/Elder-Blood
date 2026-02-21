using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
        public ShadowCaster2D shadowCaster2D;
    }
}