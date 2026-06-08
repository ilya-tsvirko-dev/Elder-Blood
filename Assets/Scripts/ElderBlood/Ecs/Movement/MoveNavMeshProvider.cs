using Scellecs.Morpeh.Providers;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace ElderBlood.Ecs.Movement
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MoveNavMeshProvider : MonoProvider<MoveNavMeshComponent> 
    {
        protected override void Initialize()
        {
            base.Initialize();
            GetData().navMeshPath = new NavMeshPath();
        }
    }
}