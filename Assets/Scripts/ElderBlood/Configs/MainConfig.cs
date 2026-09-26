using ElderBlood.Configs.Guard;
using UnityEngine;

namespace ElderBlood.Configs
{
    [CreateAssetMenu(fileName = "MainConfig", menuName = "Configs/MainConfig", order = 1)]
    public class MainConfig : ScriptableObject
    {
        public GuardConfig GuardConfig;
        public AbilityConfig AbilityConfig;
        public LevelContainer LevelContainer;
        public CharacterConfig CharacterConfig;
        public UpgradeConfig UpgradeConfig;
        public VillageConfig VillageConfig;
        public LogConfig LogConfig;
        public AudioConfig AudioConfig;
    }
}