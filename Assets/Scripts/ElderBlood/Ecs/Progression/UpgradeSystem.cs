using ElderBlood.Configs;
using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Movement;
using ElderBlood.Save;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Progression
{
    // God-of-Blood vendor (v1, no UI): one input key buys the next entry from the ordered UpgradeConfig
    // catalog while the player stands near a vendor. Spends the live blood pool; persists via SaveService.
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UpgradeSystem : ISystem
    {
        public World World { get; set; }

        private Filter _playerFilter;
        private Filter _vendorFilter;
        private Stash<BloodComponent> _bloodStash;
        private Stash<ShieldComponent> _shieldStash;
        private Stash<MoveComponent> _moveStash;
        private Stash<PlayerProgressComponent> _progressStash;
        private Stash<GodOfBloodComponent> _vendorStash;

        private Event<UpgradeEvent> _upgradeEvent;

        private readonly UpgradeConfig _config;
        private readonly SaveService _saveService;

        public UpgradeSystem(MainConfig config, SaveService saveService)
        {
            _config = config.UpgradeConfig;
            _saveService = saveService;
        }

        public void OnAwake()
        {
            _playerFilter = World.Filter.With<BloodComponent>().With<ShieldComponent>()
                .With<MoveComponent>().With<PlayerProgressComponent>().Build();
            _vendorFilter = World.Filter.With<GodOfBloodComponent>().Build();
            _bloodStash = World.GetStash<BloodComponent>();
            _shieldStash = World.GetStash<ShieldComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _progressStash = World.GetStash<PlayerProgressComponent>();
            _vendorStash = World.GetStash<GodOfBloodComponent>();

            _upgradeEvent = World.GetEvent<UpgradeEvent>();
            _upgradeEvent.Subscribe(OnUpgrade);
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Dispose()
        {
        }

        private void OnUpgrade(FastList<UpgradeEvent> events)
        {
            foreach (var _ in events)
            {
                foreach (var player in _playerFilter)
                {
                    TryPurchase(player);
                }
            }
        }

        private void TryPurchase(Entity player)
        {
            var index = _saveService.Progress.purchasedCount;
            if (_config.upgrades == null || index >= _config.upgrades.Count)
            {
                Debug.Log("[Upgrade] Catalog exhausted: nothing left to buy");
                return;
            }

            ref var move = ref _moveStash.Get(player);
            Vector2 playerPos = move.rigidbody.position;
            if (!IsNearVendor(playerPos))
            {
                Debug.Log("[Upgrade] Not near a God-of-Blood vendor");
                return;
            }

            var entry = _config.upgrades[index];

            ref var blood = ref _bloodStash.Get(player);
            if (blood.current < entry.cost)
            {
                Debug.Log($"[Upgrade] Denied '{entry.id}' ({entry.type}): need {entry.cost} blood, have {blood.current:0.#}");
                return; // currency is the live blood pool
            }

            blood.current -= entry.cost;

            var progress = _saveService.Progress;
            ref var shield = ref _shieldStash.Get(player);
            ref var mirror = ref _progressStash.Get(player);

            switch (entry.type)
            {
                case UpgradeType.BloodCapacity:
                    blood.capacity += entry.amount;
                    progress.bloodCapacityBonus += entry.amount;
                    mirror.bloodCapacityBonus += entry.amount;
                    break;
                case UpgradeType.ShieldUnlock:
                    shield.unlocked = true;
                    shield.current = shield.max;
                    progress.shieldUnlocked = true;
                    mirror.shieldUnlocked = true;
                    break;
                case UpgradeType.ShieldMax:
                    shield.max += entry.amount;
                    shield.current += entry.amount;
                    progress.shieldMaxBonus += entry.amount;
                    mirror.shieldMaxBonus += entry.amount;
                    break;
            }

            progress.purchasedCount++;
            _saveService.Save();

            Debug.Log($"[Upgrade] Bought '{entry.id}' ({entry.type} +{entry.amount}): -{entry.cost} blood → {blood.current:0.#} left. Next index {progress.purchasedCount}/{_config.upgrades.Count}");
        }

        private bool IsNearVendor(Vector2 playerPos)
        {
            foreach (var vendor in _vendorFilter)
            {
                ref var god = ref _vendorStash.Get(vendor);
                var sqr = ((Vector2)god.transform.position - playerPos).sqrMagnitude;
                if (sqr <= god.interactRadius * god.interactRadius)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
