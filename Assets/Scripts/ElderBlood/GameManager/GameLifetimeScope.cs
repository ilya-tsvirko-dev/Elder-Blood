using ElderBlood.Configs;
using ElderBlood.Save;
using ElderBlood.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ElderBlood
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private MainMenuView _menuView;
        [SerializeField] private HudView _hudView;
        [SerializeField] private PauseView _pauseView;
        [SerializeField] private RunEndView _runEndView;
        [SerializeField] private SettingsView _settingsView;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private CameraController _camera;
        [SerializeField] private WorldCreator _worldCreator;
        [SerializeField] private MainConfig _mainConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            // Before anything else registers: the logger is global and the editor tools and the
            // pre-run code log long before WorldCreator ever builds the ECS world.
            GameLogger.Init(_mainConfig != null ? _mainConfig.LogConfig : null);

            builder.RegisterComponent(_menuView);
            builder.RegisterComponent(_hudView);
            builder.RegisterComponent(_pauseView);
            builder.RegisterComponent(_runEndView);
            builder.RegisterComponent(_levelManager);
            builder.RegisterComponent(_camera);
            builder.RegisterComponent(_worldCreator);

            builder.RegisterInstance(_mainConfig);

            builder.Register<SaveService>(Lifetime.Singleton);
            builder.Register<HudChannel>(Lifetime.Singleton);
            builder.Register<RunChannel>(Lifetime.Singleton);
            builder.Register<AudioService>(Lifetime.Singleton);

            builder.RegisterEntryPoint<MainMenuPresenter>();
            builder.RegisterEntryPoint<CameraPresenter>();
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterEntryPoint<PausePresenter>();
            builder.RegisterEntryPoint<RunEndPresenter>();

            // Guarded, not unconditional: RegisterComponent dereferences the instance to read its
            // type, so an unassigned view would take the whole container down at Configure time —
            // every screen dead, not just this one. Say so loudly instead of disappearing quietly.
            if (_settingsView != null)
            {
                builder.RegisterComponent(_settingsView);
                builder.RegisterEntryPoint<SettingsPresenter>();
            }
            else
            {
                GameLogger.Error($"[Boot] {nameof(_settingsView)} is not assigned on {name} — " +
                                 "the settings screen will not exist this session.");
            }
        }
    }
}
