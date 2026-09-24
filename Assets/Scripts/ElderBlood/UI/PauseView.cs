using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class PauseView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        public event Action ResumeClicked;
        public event Action RestartClicked;
        public event Action QuitClicked;

        public bool IsVisible => _panel != null && _panel.activeSelf;

        private void Awake()
        {
            _resumeButton.onClick.AddListener(() => ResumeClicked?.Invoke());
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
            Hide();
        }

        public void Show()
        {
            // Own object may be disabled in the scene; activating only the child panel would do nothing.
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (_panel != null)
            {
                _panel.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }
    }
}
