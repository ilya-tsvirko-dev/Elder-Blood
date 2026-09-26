using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class SettingsView : PanelView
    {
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Button _backButton;

        public event Action<float> VolumeChanged;
        public event Action BackClicked;

        protected override void Awake()
        {
            base.Awake();

            if (_volumeSlider != null)
            {
                _volumeSlider.onValueChanged.AddListener(value => VolumeChanged?.Invoke(value));
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(() => BackClicked?.Invoke());
            }
        }

        public void SetVolume(float value)
        {
            if (_volumeSlider != null)
            {
                _volumeSlider.SetValueWithoutNotify(value);
            }
        }
    }
}
