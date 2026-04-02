using DefaultNamespace.Configs.Guard;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace DefaultNamespace.Configs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ConfigSetupSystem : ISystem
    {
        private Event<ConfigSetupEvent> _setupEvents;
        public World World { get; set; }

        private MainConfig _mainConfig;

        public void OnUpdate(float deltaTime) { }

        public void OnAwake()
        {
            _setupEvents = World.GetEvent<ConfigSetupEvent>();
            _setupEvents.Subscribe(HandleDebugTriggerEvents);
            var filter = World.Filter.With<ConfigSetupComponent>().Build();
            var stash = World.GetStash<ConfigSetupComponent>();
            foreach (var entity in filter)
            {
                ref var setupComponent = ref stash.Get(entity);
                _mainConfig = setupComponent.config;
            }

            _mainConfig.OnReset += SetConfig;

            _setupEvents.NextFrame(new ConfigSetupEvent());
        }

        public void Dispose()
        {
            _mainConfig.OnReset -= SetConfig;
        }
        
        private void HandleDebugTriggerEvents(FastList<ConfigSetupEvent> eventHolders)
        {
            foreach (var setupEvent in eventHolders)
            {
                SetConfig();
            }
        }

        private void SetConfig()
        {
            World.GetEvent<GuardSetupConfigEvent>().NextFrame(new GuardSetupConfigEvent(_mainConfig.GuardConfig));
        }
    }
}