using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    // Updates each guard's "?"/"!" icon from its alert state. Only touches the renderer on change.
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardAlertIndicatorSystem : ISystem
    {
        private static readonly Color SuspiciousColor = Color.yellow;
        private static readonly Color ChaseColor = Color.red;

        public World World { get; set; }

        private Filter _filter;
        private Stash<GuardAlertComponent> _alertStash;
        private Stash<GuardAlertIndicatorComponent> _indicatorStash;

        public void OnAwake()
        {
            _filter = World.Filter.With<GuardAlertComponent>().With<GuardAlertIndicatorComponent>().Build();
            _alertStash = World.GetStash<GuardAlertComponent>();
            _indicatorStash = World.GetStash<GuardAlertIndicatorComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var alert = ref _alertStash.Get(entity);
                ref var indicator = ref _indicatorStash.Get(entity);

                if (indicator.initialized && indicator.lastState == alert.state)
                {
                    continue;
                }

                indicator.initialized = true;
                indicator.lastState = alert.state;
                Apply(ref indicator, alert.state);
            }
        }

        public void Dispose()
        {
        }

        private static void Apply(ref GuardAlertIndicatorComponent indicator, GuardAlertState state)
        {
            var renderer = indicator.renderer;
            if (renderer == null)
            {
                return;
            }

            switch (state)
            {
                case GuardAlertState.Suspicious:
                case GuardAlertState.Search:
                    renderer.sprite = indicator.suspiciousSprite;
                    renderer.color = SuspiciousColor;
                    renderer.enabled = true;
                    break;
                case GuardAlertState.Chase:
                    renderer.sprite = indicator.chaseSprite;
                    renderer.color = ChaseColor;
                    renderer.enabled = true;
                    break;
                default: // Patrol, Return
                    renderer.enabled = false;
                    break;
            }
        }
    }
}
