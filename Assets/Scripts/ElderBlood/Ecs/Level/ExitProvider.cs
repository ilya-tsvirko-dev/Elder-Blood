using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Level
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ExitProvider : MonoProvider<ExitComponent>
    {
        protected override void Initialize()
        {
            base.Initialize();
            ref var data = ref GetData();
            data.transform = transform;
        }
    }
}
