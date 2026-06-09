using ElderBlood.Ecs.Health;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Abilities
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InvisibilitySystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<InvisibilityActiveComponent> _invisibilityStash;
        private Stash<MoveComponent> _moveStash;
        private Stash<HealthShaderComponent> _shaderStash;

        public void OnAwake()
        {
            _filter = World.Filter.With<InvisibilityActiveComponent>().With<MoveComponent>()
                .With<HealthShaderComponent>().Build();
            _invisibilityStash = World.GetStash<InvisibilityActiveComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _shaderStash = World.GetStash<HealthShaderComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var invisibility = ref _invisibilityStash.Get(entity);
                invisibility.timer -= deltaTime;
                if (invisibility.timer > 0f)
                {
                    continue;
                }

                _moveStash.Get(entity).speedMultiplier = 1f;
                _shaderStash.Get(entity).isFaded = false;
                _invisibilityStash.Remove(entity);
            }
        }

        public void Dispose()
        {
        }
    }
}
