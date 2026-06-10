using ElderBlood.Ecs.Blood;
using ElderBlood.Save;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Progression
{
    // Applies persisted meta-progression to the player on spawn and writes it back on death.
    // Targets players that don't yet carry PlayerProgressComponent, so each newly spawned player
    // (including run restarts) gets progress applied exactly once.
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PlayerProgressSystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<BloodComponent> _bloodStash;
        private Stash<ShieldComponent> _shieldStash;
        private Stash<PlayerProgressComponent> _progressStash;

        private Event<PlayerDeathEvent> _deathEvent;

        private readonly SaveService _saveService;

        public PlayerProgressSystem(SaveService saveService)
        {
            _saveService = saveService;
        }

        public void OnAwake()
        {
            _filter = World.Filter.With<BloodComponent>().With<ShieldComponent>()
                .Without<PlayerProgressComponent>().Build();
            _bloodStash = World.GetStash<BloodComponent>();
            _shieldStash = World.GetStash<ShieldComponent>();
            _progressStash = World.GetStash<PlayerProgressComponent>();

            _deathEvent = World.GetEvent<PlayerDeathEvent>();
            _deathEvent.Subscribe(OnPlayerDeath);
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ApplyProgress(entity);
            }
        }

        public void Dispose()
        {
        }

        private void ApplyProgress(Entity entity)
        {
            var progress = _saveService.Progress;

            ref var blood = ref _bloodStash.Get(entity);
            blood.capacity += progress.bloodCapacityBonus;
            if (blood.current > blood.capacity)
            {
                blood.current = blood.capacity;
            }

            ref var shield = ref _shieldStash.Get(entity);
            shield.unlocked |= progress.shieldUnlocked;
            shield.max += progress.shieldMaxBonus;
            if (shield.unlocked)
            {
                shield.current = shield.max;
            }

            _progressStash.Set(entity, new PlayerProgressComponent
            {
                bloodCapacityBonus = progress.bloodCapacityBonus,
                shieldUnlocked = progress.shieldUnlocked,
                shieldMaxBonus = progress.shieldMaxBonus
            });
        }

        private void OnPlayerDeath(FastList<PlayerDeathEvent> events)
        {
            // Meta-progression persists at run end. The vendor (Phase 5c) keeps SaveService.Progress
            // updated on purchase; here we just flush it to disk.
            _saveService.Save();
        }
    }
}
