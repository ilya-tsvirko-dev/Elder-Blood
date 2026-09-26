using System;
using ElderBlood.Configs;
using ElderBlood.Ecs.Audio;
using ElderBlood.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ElderBlood.Services
{
    // Owns the sound stack: a persistent AudioSource, the master volume, and the retrigger guard.
    // Registered as a VContainer singleton, which disposes it with the scope — without that the
    // DontDestroyOnLoad object survives every scope teardown and stacks up.
    public sealed class AudioService : IDisposable
    {
        private readonly AudioConfig _config;
        private readonly GameObject _host;
        private readonly AudioSource _source;
        private readonly float[] _lastPlayedAt;

        public AudioService(MainConfig mainConfig, SaveService saveService)
        {
            _config = mainConfig.AudioConfig;
            // Seeded below zero so the first play of each sound is never swallowed by the cooldown.
            _lastPlayedAt = new float[Enum.GetValues(typeof(SfxType)).Length];
            for (var i = 0; i < _lastPlayedAt.Length; i++)
            {
                _lastPlayedAt[i] = float.NegativeInfinity;
            }

            _host = new GameObject("AudioService (persistent)");
            Object.DontDestroyOnLoad(_host);
            _source = _host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;

            if (_config != null && _config.mixerGroup != null)
            {
                _source.outputAudioMixerGroup = _config.mixerGroup;
            }

            // Applied here rather than in SettingsPresenter: the saved volume has to hold even when
            // the settings screen is absent from the scene and its presenter never runs.
            SetMasterVolume(saveService.Settings.masterVolume);
        }

        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }

        public void Play(SfxType type)
        {
            if (_config == null)
            {
                return;
            }

            var clip = _config.GetClip(type);
            if (clip == null)
            {
                return;
            }

            var index = (int)type;
            if (Time.unscaledTime - _lastPlayedAt[index] < _config.repeatCooldown)
            {
                return;
            }

            _lastPlayedAt[index] = Time.unscaledTime;
            _source.PlayOneShot(clip);
        }

        public void Dispose()
        {
            if (_host != null)
            {
                Object.Destroy(_host);
            }
        }
    }
}
