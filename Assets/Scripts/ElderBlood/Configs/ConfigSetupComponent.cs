using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Configs
{
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ConfigSetupComponent : IComponent
    {
        public MainConfig config;
    }
}