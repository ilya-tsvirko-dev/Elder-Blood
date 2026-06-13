using ElderBlood.Configs;
using ElderBlood.Save;
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
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private CameraController _camera;
        [SerializeField] private WorldCreator _worldCreator;
        [SerializeField] private MainConfig _mainConfig;

        protected override void Configure(IContainerBuilder builder)
        {
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

            builder.RegisterEntryPoint<MainMenuPresenter>();
            builder.RegisterEntryPoint<CameraPresenter>();
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterEntryPoint<PausePresenter>();
            builder.RegisterEntryPoint<RunEndPresenter>();
        }
    }
}
