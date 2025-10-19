using System;
using ElderBlood.Character.Animation;
using ElderBlood.Character.Input;
using ElderBlood.Guard;
using ElderBlood.Guard.Animation;
using ElderBlood.Guard.GuardBehavior;
using Scellecs.Morpeh;
using UnityEngine;

namespace DefaultNamespace
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
            
            this._world.AddSystemsGroup(order: 0, systemsGroup);
        }
    }
}