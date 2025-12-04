using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MoveProvider : MonoProvider<MoveComponent> 
    {
        protected override void Initialize()
        {
            base.Initialize();
            GetData().rigidbody = GetComponent<Rigidbody2D>();
        }
    }
}