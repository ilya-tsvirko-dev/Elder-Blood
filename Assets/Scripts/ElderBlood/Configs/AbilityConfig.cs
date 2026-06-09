using UnityEngine;

namespace ElderBlood.Configs
{
    [CreateAssetMenu(fileName = "AbilityConfig", menuName = "Configs/AbilityConfig", order = 3)]
    public class AbilityConfig : ScriptableObject
    {
        [Header("Invisibility")]
        public float invisibilityCost;
        public float invisibilityCooldown;
        public float invisibilityDuration;
        public float invisibilitySpeedMultiplier = 1.5f;

        [Header("Blood Puddle")]
        public float puddleCost;
        public float puddleCooldown;
        public float puddleRadius = 1f;

        [Header("Blood Sight")]
        public float bloodSightCost;
        public float bloodSightCooldown;
        public float bloodSightDuration = 5f;
        public float bloodSightRadius = 6f;
        public Color bloodSightColor = Color.red;
    }
}
