using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [RequireComponent(typeof(Collider2D))]
    public sealed class GuardBehaviorProvider : MonoProvider<GuardBehaviorComponent>
    {
        protected override void Initialize()
        {
            base.Initialize();
            ref var data = ref GetData();
            data.collider = GetComponent<Collider2D>();
        }
    }
}