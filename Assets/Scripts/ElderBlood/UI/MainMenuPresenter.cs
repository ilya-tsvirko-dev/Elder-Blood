using System;
using VContainer.Unity;

namespace ElderBlood
{
    public class MainMenuPresenter : IStartable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly LevelManager _levelManager;

        public MainMenuPresenter(MainMenuView view, LevelManager levelManager)
        {
            _view = view;
            _levelManager = levelManager;
        }

        public void Start()
        {
            _view.StartClicked += OnStartClicked;
        }

        public void Dispose()
        {
            _view.StartClicked -= OnStartClicked;
        }

        private void OnStartClicked()
        {
            _levelManager.LoadFirstLevel();
            _view.HideStartButton();
        }
    }
}
