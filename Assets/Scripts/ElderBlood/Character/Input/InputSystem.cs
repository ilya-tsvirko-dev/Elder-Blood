using ElderBlood.Character.Animation;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElderBlood.Character.Input
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class InputSystem : IFixedSystem 
    {
        public World World { get; set;}
    
        private Filter _filter;
        private Stash<InputComponent> _inputStash;
        private Stash<MoveComponent> _moveStash;
        private Stash<CharacterAnimationComponent> _animationStash;
    
        private Controls _controls;

        public void OnAwake() 
        {
            _filter = World.Filter.With<InputComponent>()
                .With<MoveComponent>().With<CharacterAnimationComponent>().Build();
            _inputStash = World.GetStash<InputComponent>();
            _moveStash = World.GetStash<MoveComponent>();
            _animationStash = World.GetStash<CharacterAnimationComponent>();

            ref var component =  ref _inputStash.Get(_filter.First());
            component.inputActions = new Controls();
            _controls = component.inputActions;
        
            _controls.Enable();
        
            _controls.Main.Run.performed += EnableRun;
            _controls.Main.Run.canceled += DisableRun;
            //
            // controls.Main.Sit.performed += Sit;
            // controls.Main.Sit.canceled += WakeUp;
            //
            // controls.Main.Jump.performed += Jump;
            // controls.Main.Hide.performed += Hide;
            // controls.Main.Magic.performed += Magic;
            // controls.Main.Death.performed += Death;
        }

        private void DisableRun(InputAction.CallbackContext obj)
        {
            foreach (var entity in _filter)
            {
                ref var moveComponent = ref _moveStash.Get(entity);
                moveComponent.isRun = false;
            }
        }

        private void EnableRun(InputAction.CallbackContext obj)
        {
            foreach (var entity in _filter)
            {
                ref var moveComponent = ref _moveStash.Get(entity);
                moveComponent.isRun = true;
            }
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                ref var inputComponent = ref _inputStash.Get(entity);
                ref var moveComponent = ref _moveStash.Get(entity);
                ref var animationComponent = ref _animationStash.Get(entity);
            
                if (moveComponent.isAction)
                {
                    return;
                }

                moveComponent.moveVector = inputComponent.inputActions.Main.Move.ReadValue<Vector2>();

                if (moveComponent.moveVector == Vector2.zero && !moveComponent.isRun)
                {
                    animationComponent.moveType = MoveAnimationType.Stop;
                    //animationComponent.moveVector = moveComponent.moveVector;
                    return;
                }

                if (moveComponent.isRun)
                {
                    animationComponent.moveType = MoveAnimationType.Run;
                    animationComponent.moveVector = moveComponent.moveVector;
                    return;
                }

                animationComponent.moveType = MoveAnimationType.Move;
                animationComponent.moveVector = moveComponent.moveVector;
            }
        }

        public void Dispose()
        {
            _controls.Disable();
        }
    }
}