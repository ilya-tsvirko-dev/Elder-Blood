using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Village
{
    // Runtime mirror of the persisted village reactivity state. Lives on a single
    // code-created entity, seeded from SaveService. Notoriety grows only (v1).
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct VillageStateComponent : IComponent
    {
        [Title("Runtime State")]
        [ReadOnly] public float notoriety;
        [ReadOnly] public int tier;
        [ReadOnly] public int drainedCount;
        [ReadOnly] public int killedCount;
    }
}
