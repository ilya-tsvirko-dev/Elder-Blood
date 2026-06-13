using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    // Drives a "?"/"!" icon above the guard from its alert state.
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GuardAlertIndicatorComponent : IComponent
    {
        public SpriteRenderer renderer;
        public Sprite suspiciousSprite; // "?" — Suspicious / Search
        public Sprite chaseSprite;      // "!" — Chase

        [Title("Runtime State")]
        [ReadOnly] public GuardAlertState lastState;
        [ReadOnly] public bool initialized;
    }
}
