using ElderBlood.Configs;
using ElderBlood.Configs.Guard;
using ElderBlood.Character.Animation;
using ElderBlood.Character.Input;
using ElderBlood.Guard;
using ElderBlood.Guard.Animation;
using ElderBlood.Guard.GuardBehavior;
using ElderBlood.Guard.GuardPatrol;
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
            systemsGroup.AddSystem(new GuardTriggerSystem());
            systemsGroup.AddSystem(new GuardBehaviorSystem());
            systemsGroup.AddSystem(new GuardAnimationSystem());
            systemsGroup.AddSystem(new GuardSetupSystem());
            systemsGroup.AddSystem(new ConfigSetupSystem());
            systemsGroup.AddSystem(new GuardPatrolSystem());

            _world.AddSystemsGroup(order: 0, systemsGroup);
        }
    }
}
