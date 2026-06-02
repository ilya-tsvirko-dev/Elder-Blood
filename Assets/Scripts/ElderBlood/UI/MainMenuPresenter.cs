using System;

namespace ElderBlood
{
    public class MainMenuPresenter : IDisposable
    {
        private readonly MainMenuView _view;
        private readonly LevelManager _levelManager;

        public MainMenuPresenter(MainMenuView view, LevelManager levelManager)
        {
            _view = view;
            _levelManager = levelManager;

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
