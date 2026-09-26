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
            _channel.ShieldChanged += OnShieldChanged;
            _channel.BloodSightChanged += OnBloodSightChanged;
        }

        public void Dispose()
        {
            _channel.BloodChanged -= OnBloodChanged;
            _channel.AbilitiesChanged -= OnAbilitiesChanged;
            _channel.ShieldChanged -= OnShieldChanged;
            _channel.BloodSightChanged -= OnBloodSightChanged;
        }

        private void OnBloodChanged(float current, float capacity) => _view.SetBlood(current, capacity);
        private void OnAbilitiesChanged(HudAbilities abilities) => _view.SetAbilities(abilities);
        private void OnShieldChanged(float current, float max, bool unlocked) => _view.SetShield(current, max, unlocked);
        private void OnBloodSightChanged(bool active, float remaining) => _view.SetBloodSight(active, remaining);
    }
}
