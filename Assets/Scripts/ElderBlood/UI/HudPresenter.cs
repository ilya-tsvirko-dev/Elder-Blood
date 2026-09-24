using System;
using VContainer.Unity;

namespace ElderBlood
{
    public class HudPresenter : IStartable, IDisposable
    {
        private readonly HudView _view;
        private readonly HudChannel _channel;

        public HudPresenter(HudView view, HudChannel channel)
        {
            _view = view;
            _channel = channel;
        }

        public void Start()
        {
            _channel.BloodChanged += OnBloodChanged;
            _channel.AbilitiesChanged += OnAbilitiesChanged;
        }

        public void Dispose()
        {
            _channel.BloodChanged -= OnBloodChanged;
            _channel.AbilitiesChanged -= OnAbilitiesChanged;
        }

        private void OnBloodChanged(float current, float capacity) => _view.SetBlood(current, capacity);
        private void OnAbilitiesChanged(HudAbilities abilities) => _view.SetAbilities(abilities);
    }
}
