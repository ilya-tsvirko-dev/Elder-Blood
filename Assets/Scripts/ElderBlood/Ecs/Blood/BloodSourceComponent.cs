using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Blood
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct BloodSourceComponent : IComponent
    {
        public Transform transform;
        public SpriteRenderer renderer;
        public float amount;
        public SourceState state;

        // Runtime highlight state (Blood Sight).
        public bool highlighted;
        public Color baseColor;
    }
}
