using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Blood
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ShieldComponent : IComponent
    {
        public float max;
        public bool unlocked;

        [Title("Runtime State")]
        [ReadOnly] public float current;
    }
}
