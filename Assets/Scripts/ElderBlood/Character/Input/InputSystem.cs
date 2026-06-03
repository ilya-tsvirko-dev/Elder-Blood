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
        private Stash<MoveComponent> _moveStash;
        private Stash<CharacterAnimationComponent> _animationStash;
        private Stash<HealthShaderComponent> _healthShaderStash;
    
        private Controls _controls;

        public void OnAwake() 
        {
            _filter = World.Filter.With<InputComponent>()
                .With<MoveComponent>().With<CharacterAnimationComponent>().Build();
            _moveStash = World.GetStash<MoveComponent>();
            _animationStash = World.GetStash<CharacterAnimationComponent>();
            _healthShaderStash = World.GetStash<HealthShaderComponent>();

            _controls = new Controls();
            _controls.Enable();
        
            _controls.Main.Run.performed += EnableRun;
            _controls.Main.Run.canceled += DisableRun;

            _controls.Main.Sit.performed += Sit;
            _controls.Main.Sit.canceled += WakeUp;
            _controls.Main.Jump.performed += Jump;
            _controls.Main.Hide.performed += Hide;
            _controls.Main.Magic.performed += Magic;
            _controls.Main.Death.performed += Death;
            _controls.Main.Fade.performed += Fade;
        }

        private void Sit(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Sit);
        private void WakeUp(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.WakeUp);
        private void Jump(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Jump);
        private void Hide(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Hide);
        private void Magic(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Magic);
        private void Death(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Death);

        private void SetAction(CharacterAnimationType type)
        {
            foreach (var entity in _filter)
            {
                ref var moveComponent = ref _moveStash.Get(entity);
                ref var animationComponent = ref _animationStash.Get(entity);
                moveComponent.isAction = true;
                animationComponent.type = type;
            }
        }

        // Fade is a stealth toggle: it lowers the character's shader alpha (HealthShaderSystem),
        // it is not an animation. Hide (above) is a separate animation action.
        private void Fade(InputAction.CallbackContext obj)
        {
            foreach (var entity in _filter)
            {
                if (!_healthShaderStash.Has(entity))
                {
                    continue;
                }

                ref var healthShaderComponent = ref _healthShaderStash.Get(entity);
                healthShaderComponent.isFaded = !healthShaderComponent.isFaded;
            }
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
                ref var moveComponent = ref _moveStash.Get(entity);
                ref var animationComponent = ref _animationStash.Get(entity);
            
                if (moveComponent.isAction)
                {
                    return;
                }

                moveComponent.moveVector = _controls.Main.Move.ReadValue<Vector2>();

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