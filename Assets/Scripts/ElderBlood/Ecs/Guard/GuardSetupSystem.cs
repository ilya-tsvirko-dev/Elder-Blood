using ElderBlood.Configs;
using ElderBlood.Configs.Guard;
using ElderBlood.Ecs.Health;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardSetupSystem : ISystem
    {
        private readonly GuardConfig _config;

        private Filter _filter;
        private Stash<HealthComponent> _stash;
        private Stash<GuardConfiguredComponent> _configuredStash;

        public World World { get; set; }

        public GuardSetupSystem(MainConfig config)
        {
            _config = config.GuardConfig;
        }

        public void OnAwake()
        {
            _filter = World.Filter.With<HealthComponent>().With<GuardComponent>()
                .Without<GuardConfiguredComponent>().Build();
            _stash = World.GetStash<HealthComponent>();
            _configuredStash = World.GetStash<GuardConfiguredComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var component = ref _stash.Get(entity);
                component.maxHealthPoint = _config.MaxHealth;
                component.healthPoint = _config.MaxHealth;
                _configuredStash.Set(entity);
            }
        }

        public void Dispose()
        {
        }
    }
}
