using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Health
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct HealthShaderComponent : IComponent
    {
        public Material material;

        [Title("Runtime State")]
        [ReadOnly] public float fillAmount;
        [ReadOnly] public float neededFillAmount;
        [ReadOnly] public bool isFaded;
        [ReadOnly] public float alpha;
    }
}