using ElderBlood.Configs;
using ElderBlood.Ecs.Blood;
using ElderBlood.Save;
using ElderBlood.Services;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Village
{
    // Grows village notoriety from drain/kill events, advances tiers, and wakes sleeping
    // sources when a tier is reached (woken sources become non-drainable). Persists via SaveService.
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class VillageReactionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _villageFilter;
        private Filter _sourceFilter;
        private Stash<VillageStateComponent> _villageStash;
        private Stash<BloodSourceComponent> _sourceStash;

        private Event<SourceDrainedEvent> _drainedEvent;

        private readonly VillageConfig _config;
        private readonly SaveService _saveService;

        private Entity _villageEntity;

        public VillageReactionSystem(MainConfig config, SaveService saveService)
        {
            _config = config.VillageConfig;
            _saveService = saveService;
        }

        public void OnAwake()
        {
            _villageFilter = World.Filter.With<VillageStateComponent>().Build();
            _sourceFilter = World.Filter.With<BloodSourceComponent>().Build();
            _villageStash = World.GetStash<VillageStateComponent>();
            _sourceStash = World.GetStash<BloodSourceComponent>();

            _drainedEvent = World.GetEvent<SourceDrainedEvent>();
            _drainedEvent.Subscribe(OnSourceDrained);

            EnsureVillageEntity();
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Dispose()
        {
        }

        private void EnsureVillageEntity()
        {
            // World.Default persists across CreateWorld() calls, so reuse an existing village entity.
            foreach (var entity in _villageFilter)
            {
                _villageEntity = entity;
                return;
            }

            var saved = _saveService.Village;
            _villageEntity = World.CreateEntity();
            _villageStash.Set(_villageEntity, new VillageStateComponent
            {
                notoriety = saved.notoriety,
                tier = saved.tier,
                drainedCount = saved.drainedCount,
                killedCount = saved.killedCount
            });

            GameLogger.Village($"[Village] Loaded: notoriety {saved.notoriety:0.#}, tier {saved.tier}, drained {saved.drainedCount}");
        }

        private void OnSourceDrained(FastList<SourceDrainedEvent> events)
        {
            foreach (var _ in events)
            {
                ref var village = ref _villageStash.Get(_villageEntity);

                village.drainedCount++;
                village.notoriety += _config.notorietyPerDrain;

                var newTier = _config.notorietyPerTier > 0f
                    ? Mathf.FloorToInt(village.notoriety / _config.notorietyPerTier)
                    : village.tier;

                GameLogger.Village($"[Village] Notoriety {village.notoriety:0.#} (tier {village.tier}), drained {village.drainedCount}");

                if (newTier > village.tier)
                {
                    village.tier = newTier;
                    WakeSources();
                    GameLogger.Village($"[Village] Tier up → {village.tier}");
                }

                Persist(village);
                _saveService.Save();
            }
        }

        private void WakeSources()
        {
            var woken = 0;
            foreach (var sourceEntity in _sourceFilter)
            {
                ref var source = ref _sourceStash.Get(sourceEntity);
                if (source.state == SourceState.Asleep)
                {
                    source.state = SourceState.Awake;
                    woken++;
                }
            }

            GameLogger.Village($"[Village] Woke {woken} source(s) — no longer drainable");
        }

        private void Persist(in VillageStateComponent village)
        {
            var saved = _saveService.Village;
            saved.notoriety = village.notoriety;
            saved.tier = village.tier;
            saved.drainedCount = village.drainedCount;
            saved.killedCount = village.killedCount;
        }
    }
}
