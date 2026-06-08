using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Health
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct HealthComponent : IComponent
    {
        public int maxHealthPoint;

        [Title("Runtime State")]
        [ReadOnly] public int healthPoint;
    }
}