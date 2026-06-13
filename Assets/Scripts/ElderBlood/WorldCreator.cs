using ElderBlood.Configs;
using ElderBlood.Ecs.Abilities;
using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Character;
using ElderBlood.Ecs.Guard;
using ElderBlood.Ecs.Health;
using ElderBlood.Ecs.Hud;
using ElderBlood.Ecs.Level;
using ElderBlood.Ecs.Movement;
using ElderBlood.Ecs.Progression;
using ElderBlood.Ecs.Village;
using ElderBlood.Save;
using Scellecs.Morpeh;
using UnityEngine;
using VContainer;

namespace ElderBlood
{
    public class WorldCreator : MonoBehaviour
    {
        private World _world;
        private MainConfig _mainConfig;
        private SaveService _saveService;
        private LevelManager _levelManager;
        private HudChannel _hudChannel;
        private bool _created;

        [Inject]
        public void Construct(MainConfig mainConfig, SaveService saveService, LevelManager levelManager, HudChannel hudChannel)
        {
            _mainConfig = mainConfig;
            _saveService = saveService;
            _levelManager = levelManager;
            _hudChannel = hudChannel;
        }

        public void CreateWorld()
        {
            // Systems are created once and reused across floors; World.Default persists and filters
            // pick up newly spawned entities automatically.
            if (_created)
            {
                return;
            }

            _created = true;
            _world = World.Default;

            var systemsGroup = _world.CreateSystemsGroup();
            systemsGroup.AddSystem(new HealthShaderSystem());
            systemsGroup.AddSystem(new InputSystem());
            systemsGroup.AddSystem(new MoveSystem());
            systemsGroup.AddSystem(new CharacterAnimationSystem());
            systemsGroup.AddSystem(new AbilitySystem(_mainConfig));
            systemsGroup.AddSystem(new InvisibilitySystem());
            systemsGroup.AddSystem(new BloodPuddleSystem());
            systemsGroup.AddSystem(new BloodSightSystem());
            systemsGroup.AddSystem(new GuardVisionSystem());
            systemsGroup.AddSystem(new GuardAlertSystem());
            systemsGroup.AddSystem(new GuardBehaviorSystem());
            systemsGroup.AddSystem(new GuardAnimationSystem());
            systemsGroup.AddSystem(new GuardAlertIndicatorSystem());
            systemsGroup.AddSystem(new GuardSetupSystem(_mainConfig));
            systemsGroup.AddSystem(new GuardPatrolSystem());
            systemsGroup.AddSystem(new PlayerProgressSystem(_saveService));
            systemsGroup.AddSystem(new UpgradeSystem(_mainConfig, _saveService));
            systemsGroup.AddSystem(new DamageSystem());
            systemsGroup.AddSystem(new DrainSystem());
            systemsGroup.AddSystem(new ObjectiveSystem(_levelManager));
            systemsGroup.AddSystem(new HudFeedSystem(_hudChannel, _mainConfig));
            // Village reactivity disabled for now (pending rework/removal).
            // systemsGroup.AddSystem(new VillageReactionSystem(_mainConfig, _saveService));

            _world.AddSystemsGroup(order: 0, systemsGroup);
        }
    }
}
