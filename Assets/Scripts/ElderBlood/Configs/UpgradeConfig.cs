using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElderBlood.Configs
{
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Configs/UpgradeConfig", order = 4)]
    public class UpgradeConfig : ScriptableObject
    {
        // Ordered catalog: the vendor sells entries top-to-bottom. Tiers = several entries of the same type in a row.
        public List<UpgradeEntry> upgrades = new();

        [Serializable]
        public class UpgradeEntry
        {
            public string id;
            public UpgradeType type;
            public float cost;   // blood spent (the live pool)
            public float amount; // effect magnitude; ignored for ShieldUnlock
        }
    }
}
