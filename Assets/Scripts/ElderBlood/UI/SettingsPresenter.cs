using System;
using ElderBlood.Save;
using ElderBlood.Services;
using VContainer.Unity;

namespace ElderBlood
{
    public class SettingsPresenter : IStartable, IDisposable
    {
        private readonly SettingsView _view;
        private readonly PauseView _pauseView;
        private readonly SaveService _saveService;
        private readonly AudioService _audioService;

        private bool _dirty;

        public SettingsPresenter(SettingsView view, PauseView pauseView, SaveService saveService,
            AudioService audioService)
        {
            _view = view;
            _pauseView = pauseView;
            _saveService = saveService;
            _audioService = audioService;
        }

        public void Start()
        {
            _view.SetVolume(_saveService.Settings.masterVolume);
            _view.VolumeChanged += OnVolumeChanged;
            _view.BackClicked += CloseSettings;

            _pauseView.SettingsClicked += OnSettingsClicked;

            // Resuming or restarting from the pause menu must not leave the settings panel on screen.
            _pauseView.Closed += CloseSettings;
        }

        public void Dispose()
        {
            _view.VolumeChanged -= OnVolumeChanged;
            _view.BackClicked -= CloseSettings;

            _pauseView.SettingsClicked -= OnSettingsClicked;
            _pauseView.Closed -= CloseSettings;

            Flush();
        }

        private void OnSettingsClicked()
        {
            _view.SetVolume(_saveService.Settings.masterVolume);
            _view.Show();
        }

        // Dragging a slider fires every frame; writing save.json each time would hammer the disk.
        // The value is applied live and persisted once, when the screen closes.
        private void OnVolumeChanged(float value)
        {
            _audioService.SetMasterVolume(value);
            _saveService.Settings.masterVolume = value;
            _dirty = true;
        }

        private void CloseSettings()
        {
            _view.Hide();
            Flush();
        }

        private void Flush()
        {
            if (!_dirty)
            {
                return;
            }

            _dirty = false;
            _saveService.Save();
        }
    }
}
