using UnityEngine;

namespace ElderBlood.Configs
{
    [CreateAssetMenu(fileName = "VillageConfig", menuName = "Configs/VillageConfig", order = 5)]
    public class VillageConfig : ScriptableObject
    {
        public float notorietyPerDrain = 1f;  // notoriety added per successful drain
        public float notorietyPerTier = 3f;    // notoriety needed to advance one tier
    }
}
