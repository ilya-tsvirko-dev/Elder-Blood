using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class RunEndView : PanelView
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;

        public event Action RestartClicked;
        public event Action QuitClicked;

        protected override void Awake()
        {
            base.Awake();

            _restartButton.onClick.AddListener(() => RestartClicked?.Invoke());
            _quitButton.onClick.AddListener(() => QuitClicked?.Invoke());
        }

        public void Show(string title)
        {
            if (_title != null)
            {
                _title.text = title;
            }

            Show();
        }
    }
}
