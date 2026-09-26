using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class PauseView : PanelView
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        public event Action ResumeClicked;
        public event Action RestartClicked;
        public event Action SettingsClicked;
        public event Action QuitClicked;

        // Raised on Hide() so panels opened from here (settings) can close with it.
        public event Action Closed;

        protected override void Awake()
        {
            base.Awake();

            _resumeButton.onClick.AddListener(() => ResumeClicked?.Invoke());
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _quitButton.onClick.AddListener(() => QuitClicked?.Invoke());

            // Optional: the settings screen is a separate panel and may not exist in every scene.
            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(() => SettingsClicked?.Invoke());
            }
        }

        public override void Hide()
        {
            base.Hide();
            Closed?.Invoke();
        }
    }
}
