using ElderBlood.Guard;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace DefaultNamespace.Configs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardSetupSystem : ISystem
    {
        private Filter _filter;
        private Stash<HealthComponent> _stash;
        
        private Event<GuardSetupConfigEvent> _setupEvents;
        
        public World World { get; set; }
        
        public void OnAwake()
        {
            _filter = World.Filter.With<HealthComponent>().With<GuardComponent>().Build();
            _stash = World.GetStash<HealthComponent>();

            _setupEvents = World.GetEvent<GuardSetupConfigEvent>();
            _setupEvents.Subscribe(Setup);
        }
        
        public void OnUpdate(float deltaTime) { }
        public void Dispose() { }

        private void Setup(FastList<GuardSetupConfigEvent> eventHolders)
        {
            foreach (var eventHolder in eventHolders)
            {
                foreach (var entity in _filter)
                {
                    ref var component = ref _stash.Get(entity);

                    component.maxHealthPoint = eventHolder.Config.MaxHealth;
                    component.healthPoint = eventHolder.Config.MaxHealth;
                }
            }
        }
    }
}