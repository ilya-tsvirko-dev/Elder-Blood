using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Abilities
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct AbilitiesComponent : IComponent
    {
        [Title("Runtime State")]
        [ReadOnly] public float invisibilityCooldown;
        [ReadOnly] public float puddleCooldown;
        [ReadOnly] public float bloodSightCooldown;
    }
}
