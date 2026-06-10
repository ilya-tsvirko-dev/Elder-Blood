using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Guard
{
    // Tag marking a guard whose stats were already applied from config, so GuardSetupSystem
    // configures each spawned guard exactly once (including guards on later floors).
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GuardConfiguredComponent : IComponent
    {
    }
}
