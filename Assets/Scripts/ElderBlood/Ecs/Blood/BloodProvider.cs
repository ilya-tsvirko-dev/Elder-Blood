using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Blood
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BloodProvider : MonoProvider<BloodComponent>
    {
        protected override void Initialize()
        {
            base.Initialize();
            ref var data = ref GetData();
            data.current = data.capacity * 0.5f;
        }
    }
}
