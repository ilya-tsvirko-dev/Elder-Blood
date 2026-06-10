using Scellecs.Morpeh;

namespace ElderBlood.Ecs.Blood
{
    // Raised by DrainSystem when a source is actually drained (not on a failed interact press).
    public struct SourceDrainedEvent : IEventData
    {
        public float amount;
    }
}
