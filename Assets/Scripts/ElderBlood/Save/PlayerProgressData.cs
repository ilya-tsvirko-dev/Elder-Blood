using System;

namespace ElderBlood.Save
{
    // Persisted meta-progression. Stores BONUSES applied on top of the prefab's base values,
    // so base tuning stays in the prefab providers and never needs to be captured into the save file.
    [Serializable]
    public class PlayerProgressData
    {
        public float bloodCapacityBonus;
        public bool shieldUnlocked;
        public float shieldMaxBonus;

        // Number of entries already bought from the ordered UpgradeConfig catalog (sequential vendor, v1).
        public int purchasedCount;
    }
}
