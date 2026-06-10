using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Progression
{
    // Runtime mirror of the persisted meta-progression on the player entity.
    // Seeded from SaveService at spawn; the God-of-Blood vendor (Phase 5c) mutates it on purchase.
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct PlayerProgressComponent : IComponent
    {
        [Title("Runtime State")]
        [ReadOnly] public float bloodCapacityBonus;
        [ReadOnly] public bool shieldUnlocked;
        [ReadOnly] public float shieldMaxBonus;
    }
}
