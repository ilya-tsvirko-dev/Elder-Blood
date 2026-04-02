using ElderBlood.Character;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
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

        [Title("Runtime State")]
        [ReadOnly] public GuardAnimationType type;
        [ReadOnly] public MoveAnimationType moveType;
        [ReadOnly] public Vector2 moveVector;
        [ReadOnly] public float animationTime;
    }
}