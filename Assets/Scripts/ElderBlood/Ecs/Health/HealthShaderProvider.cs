using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Health
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HealthShaderProvider : MonoProvider<HealthShaderComponent>
    {
        protected override void Initialize()
        {
            base.Initialize();
            ref var data = ref GetData();
            data.material = GetComponent<SpriteRenderer>().material;
            data.alpha = 1f;
        }
    }
}