using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

namespace ElderBlood
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    
    public sealed class MoveNavMeshSystem : ISystem
    {
        private Filter _filter;
        private Stash<MoveNavMeshComponent> _navigateStash;
        private Stash<MoveComponent> _moveStash;
        
        public World World { get; set; }
        
        public void OnAwake()
        {
            _filter = World.Filter.With<MoveNavMeshComponent>().With<MoveComponent>().Build();
            _navigateStash = World.GetStash<MoveNavMeshComponent>();
            _moveStash = World.GetStash<MoveComponent>();
        }
        
        public void OnUpdate(float deltaTime)
        {

        }
        
        public void Dispose()
        {
            
        }
    }
}