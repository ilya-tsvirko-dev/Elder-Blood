using ElderBlood.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ElderBlood
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private MainMenuView _menuView;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private CameraController _camera;
        [SerializeField] private WorldCreator _worldCreator;
        [SerializeField] private MainConfig _mainConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_menuView);
            builder.RegisterComponent(_levelManager);
            builder.RegisterComponent(_camera);
            builder.RegisterComponent(_worldCreator);

            builder.RegisterInstance(_mainConfig);

            builder.RegisterEntryPoint<MainMenuPresenter>();
            builder.RegisterEntryPoint<CameraPresenter>();
        }
    }
}
