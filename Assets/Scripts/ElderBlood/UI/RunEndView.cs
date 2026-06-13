using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class RunEndView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        public event Action RestartClicked;
        public event Action QuitClicked;

        private void Awake()
        {
            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
            Hide();
        }

        public void Show(string title)
        {
            if (_title != null)
            {
                _title.text = title;
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
