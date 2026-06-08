using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Character;
using ElderBlood.Ecs.Config;
using ElderBlood.Ecs.Guard;
using ElderBlood.Ecs.Health;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using UnityEngine;

namespace ElderBlood
{
    public class WorldCreator : MonoBehaviour
    {
        private World _world;

        public void CreateWorld()
        {
            _world = World.Default;

            var systemsGroup = _world.CreateSystemsGroup();
            systemsGroup.AddSystem(new HealthShaderSystem());
            systemsGroup.AddSystem(new InputSystem());
            systemsGroup.AddSystem(new MoveSystem());
            systemsGroup.AddSystem(new CharacterAnimationSystem());
            systemsGroup.AddSystem(new GuardVisionSystem());
            systemsGroup.AddSystem(new GuardAlertSystem());
            systemsGroup.AddSystem(new GuardBehaviorSystem());
            systemsGroup.AddSystem(new GuardAnimationSystem());
            systemsGroup.AddSystem(new GuardSetupSystem());
            systemsGroup.AddSystem(new ConfigSetupSystem());
            systemsGroup.AddSystem(new GuardPatrolSystem());
            systemsGroup.AddSystem(new DamageSystem());
            systemsGroup.AddSystem(new DrainSystem());

            _world.AddSystemsGroup(order: 0, systemsGroup);
        }
    }
}
