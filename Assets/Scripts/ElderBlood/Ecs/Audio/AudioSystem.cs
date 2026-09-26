using ElderBlood.Services;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace ElderBlood.Ecs.Audio
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AudioSystem : ISystem
    {
        public World World { get; set; }

        private readonly AudioService _audioService;

        private Event<SfxEvent> _sfxEvent;

        public AudioSystem(AudioService audioService)
        {
            _audioService = audioService;
        }

        public void OnAwake()
        {
            _sfxEvent = World.GetEvent<SfxEvent>();
            _sfxEvent.Subscribe(OnSfx);
        }

        public void OnUpdate(float deltaTime)
        {
        }

        public void Dispose()
        {
        }

        private void OnSfx(FastList<SfxEvent> events)
        {
            foreach (var e in events)
            {
                _audioService.Play(e.type);
            }
        }
    }
}