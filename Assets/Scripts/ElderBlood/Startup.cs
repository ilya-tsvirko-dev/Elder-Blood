using System;
using DefaultNamespace.Configs;
using DefaultNamespace.Configs.Guard;
using ElderBlood;
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
    public class Startup : MonoBehaviour
    {
        private World _world;
        
        private void Start()
        {
            this._world = World.Default;

            var systemsGroup = this._world.CreateSystemsGroup();
            systemsGroup.AddSystem(new HealthShaderSystem());
            systemsGroup.AddSystem(new InputSystem());
            systemsGroup.AddSystem(new MoveSystem());
            systemsGroup.AddSystem(new CharacterAnimationSystem());
            systemsGroup.AddSystem(new GuardTriggerSystem());
            systemsGroup.AddSystem(new GuardBehaviorSystem());
            systemsGroup.AddSystem(new GuardAnimationSystem());
            systemsGroup.AddSystem(new GuardSetupSystem());
            systemsGroup.AddSystem(new ConfigSetupSystem());
            //systemsGroup.AddSystem(new MoveNavMeshSystem());
            systemsGroup.AddSystem(new GuardPatrolSystem());
            
            this._world.AddSystemsGroup(order: 0, systemsGroup);
        }
    }
}