using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Movement
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    
    public class MoveSystem : IFixedSystem 
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<MoveComponent> _stash;

        public void OnAwake()
        {
            _filter = World.Filter.With<MoveComponent>().Build();
            _stash = World.GetStash<MoveComponent>();
        }
        
        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var moveComponent = ref _stash.Get(entity);
                
                if (moveComponent.isAction)
                {
                    moveComponent.rigidbody.linearVelocity = Vector2.zero;
                    continue;
                }
                
                if (moveComponent.moveVector == Vector2.zero && !moveComponent.isRun)
                {
                    moveComponent.rigidbody.linearVelocity = Vector2.zero;
                    continue;
                }
                
                var speed = moveComponent.isRun ? moveComponent.runSpeed : moveComponent.speed;
                var vector = moveComponent.moveVector * (deltaTime * speed);
                moveComponent.rigidbody.linearVelocity = vector;
            }
        }
        
        public void Dispose()
        {
            
        }
    }
}