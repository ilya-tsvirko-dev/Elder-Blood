using ElderBlood.Ecs.Abilities;
using ElderBlood.Ecs.Blood;
using ElderBlood.Ecs.Movement;
using ElderBlood.Ecs.Progression;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElderBlood.Ecs.Character
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

        private Controls _controls;
        private Event<DrainEvent> _drainEvent;
        private Event<AbilityCastEvent> _castEvent;
        private Event<UpgradeEvent> _upgradeEvent;

        public void OnAwake() 
        {
            _filter = World.Filter.With<InputComponent>()
                .With<MoveComponent>().With<CharacterAnimationComponent>().Build();
            _moveStash = World.GetStash<MoveComponent>();
            _animationStash = World.GetStash<CharacterAnimationComponent>();

            _drainEvent = World.GetEvent<DrainEvent>();
            _castEvent = World.GetEvent<AbilityCastEvent>();
            _upgradeEvent = World.GetEvent<UpgradeEvent>();

            _controls = new Controls();
            _controls.Enable();
        
            _controls.Main.Run.performed += EnableRun;
            _controls.Main.Run.canceled += DisableRun;

            _controls.Main.Jump.performed += Jump;
            _controls.Main.Hide.performed += Hide;
            _controls.Main.Magic.performed += Magic;
            _controls.Main.Interact.performed += Interact;
            _controls.Main.Invisibility.performed += OnInvisibility;
            _controls.Main.BloodPuddle.performed += OnBloodPuddle;
            _controls.Main.BloodSight.performed += OnBloodSight;
            _controls.Main.Upgrade.performed += OnUpgrade;
        }

        private void Interact(InputAction.CallbackContext obj) => _drainEvent.NextFrame(new DrainEvent());
        private void OnUpgrade(InputAction.CallbackContext obj) => _upgradeEvent.NextFrame(new UpgradeEvent());
        private void OnInvisibility(InputAction.CallbackContext obj) => _castEvent.NextFrame(new AbilityCastEvent { type = AbilityType.Invisibility });
        private void OnBloodPuddle(InputAction.CallbackContext obj) => _castEvent.NextFrame(new AbilityCastEvent { type = AbilityType.BloodPuddle });
        private void OnBloodSight(InputAction.CallbackContext obj) => _castEvent.NextFrame(new AbilityCastEvent { type = AbilityType.BloodSight });

        private void Jump(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Jump);
        private void Hide(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Hide);
        private void Magic(InputAction.CallbackContext obj) => SetAction(CharacterAnimationType.Magic);

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