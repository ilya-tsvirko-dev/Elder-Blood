using Scellecs.Morpeh;

namespace ElderBlood.Ecs.Blood
{
    // v1: player-pool damage. Generalize to a targeted event when guards become damageable (Phase 4).
    public struct DamageEvent : IEventData
    {
        public float amount;
    }
}
