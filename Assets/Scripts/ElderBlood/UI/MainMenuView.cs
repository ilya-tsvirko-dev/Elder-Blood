using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button _startButton;

        public event Action StartClicked;

        private void Awake()
        {
            _startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
        }

        public void HideStartButton()
        {
            _startButton.gameObject.SetActive(false);
        }

        private void OnStartClicked()
        {
            StartClicked?.Invoke();
        }
    }
}
