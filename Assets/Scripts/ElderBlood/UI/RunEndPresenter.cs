using System;
using UnityEngine;
using VContainer.Unity;

namespace ElderBlood
{
    public class RunEndPresenter : IStartable, IDisposable
    {
        private const string WinTitle = "Victory";
        private const string DeathTitle = "You Died";

        private readonly RunEndView _view;
        private readonly RunChannel _channel;
        private readonly LevelManager _levelManager;

        public RunEndPresenter(RunEndView view, RunChannel channel, LevelManager levelManager)
        {
            _view = view;
            _channel = channel;
            _levelManager = levelManager;
        }

        public void Start()
        {
            _channel.RunWon += OnRunWon;
            _channel.RunLost += OnRunLost;
            _view.RestartClicked += OnRestart;
            _view.QuitClicked += OnQuit;
        }

        public void Dispose()
        {
            _channel.RunWon -= OnRunWon;
            _channel.RunLost -= OnRunLost;
            _view.RestartClicked -= OnRestart;
            _view.QuitClicked -= OnQuit;
            Time.timeScale = 1f; // never leave the game frozen
        }

        private void OnRunWon() => ShowOutcome(WinTitle);
        private void OnRunLost() => ShowOutcome(DeathTitle);

        private void ShowOutcome(string title)
        {
            Time.timeScale = 0f;
            _view.Show(title);
        }

        private void OnRestart()
        {
            Time.timeScale = 1f;
            _view.Hide();
            _levelManager.RestartRun();
        }

        private void OnQuit()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
