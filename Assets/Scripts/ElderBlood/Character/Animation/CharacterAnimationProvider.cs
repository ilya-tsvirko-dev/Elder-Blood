using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Character.Animation
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [RequireComponent(typeof(Animator))]
    public sealed class CharacterAnimationProvider : MonoProvider<CharacterAnimationComponent> 
    {
        protected override void Initialize()
        {
            base.Initialize();
            GetData().animator = GetComponent<Animator>();
        }
    }
}