using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace ElderBlood
{
    public class PausePresenter : IStartable, IDisposable
    {
        private readonly PauseView _view;
        private readonly LevelManager _levelManager;

        private Controls _controls;
        private bool _paused;

        public PausePresenter(PauseView view, LevelManager levelManager)
        {
            _view = view;
            _levelManager = levelManager;
        }

        public void Start()
        {
            _controls = new Controls();
            _controls.Enable();
            _controls.Main.Pause.performed += OnPause;

            _view.ResumeClicked += Resume;
            _view.RestartClicked += OnRestart;
            _view.QuitClicked += OnQuit;
        }

        public void Dispose()
        {
            if (_controls != null)
            {
                _controls.Main.Pause.performed -= OnPause;
                _controls.Disable();
            }

            _view.ResumeClicked -= Resume;
            _view.RestartClicked -= OnRestart;
            _view.QuitClicked -= OnQuit;
            Time.timeScale = 1f; // never leave the game frozen
        }

        private void OnPause(InputAction.CallbackContext context) => Toggle();

        private void Toggle()
        {
            if (_paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        private void Pause()
        {
            _paused = true;
            Time.timeScale = 0f;
            _view.Show();
        }

        private void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            _view.Hide();
        }

        private void OnRestart()
        {
            Resume();
            _levelManager.RestartRun();
        }

        private void OnQuit()
        {
            Resume();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
