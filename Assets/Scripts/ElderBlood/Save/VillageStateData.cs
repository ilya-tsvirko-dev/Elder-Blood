using System;

namespace ElderBlood.Save
{
    // Persisted village reactivity meta. Notoriety grows only (no recovery in v1).
    [Serializable]
    public class VillageStateData
    {
        public float notoriety;
        public int tier;
        public int drainedCount;
        public int killedCount;
    }
}
