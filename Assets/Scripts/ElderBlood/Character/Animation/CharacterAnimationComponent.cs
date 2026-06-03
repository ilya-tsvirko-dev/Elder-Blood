using Scellecs.Morpeh;
using Sirenix.OdinInspector;
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
        public ShadowCaster2D shadowCaster2D;

        [Title("Runtime State")]
        [ReadOnly] public CharacterAnimationType type;
        [ReadOnly] public MoveAnimationType moveType;
        [ReadOnly] public Vector2 moveVector;
        [ReadOnly] public float actionTimer;
    }
}