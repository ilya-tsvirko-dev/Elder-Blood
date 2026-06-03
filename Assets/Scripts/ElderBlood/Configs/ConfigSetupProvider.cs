using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Configs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class ConfigSetupProvider : MonoProvider<ConfigSetupComponent> { }
}