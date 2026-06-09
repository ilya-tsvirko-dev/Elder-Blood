using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Blood
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BloodSourceProvider : MonoProvider<BloodSourceComponent>
    {
        protected override void Initialize()
        {
            base.Initialize();
            ref var data = ref GetData();
            data.transform = transform;
            data.renderer = GetComponentInChildren<SpriteRenderer>();
        }
    }
}
