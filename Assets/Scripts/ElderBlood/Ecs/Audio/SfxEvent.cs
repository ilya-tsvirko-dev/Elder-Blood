using Scellecs.Morpeh;

namespace ElderBlood.Ecs.Audio
{
    public enum SfxType
    {
        Damage,
        Alert,
        AbilityCast,
        Drain
    }

    // One-shot sound request. AudioSystem is the only consumer.
    public struct SfxEvent : IEventData
    {
        public SfxType type;
    }
}
