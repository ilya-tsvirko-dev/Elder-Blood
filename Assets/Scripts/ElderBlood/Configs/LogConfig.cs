using UnityEngine;

namespace ElderBlood.Configs
{
    [CreateAssetMenu(fileName = "LogConfig", menuName = "Configs/LogConfig", order = 10)]
    public class LogConfig : ScriptableObject
    {
        [Header("Game flow")]
        public bool logRun = true;

        [Header("ECS domains")]
        public bool logDamage = true;
        public bool logGuard = true;
        public bool logBlood = true;
        public bool logAbility = true;
        public bool logUpgrade = true;
        public bool logVillage = true;
        public bool logHud = true;

        [Header("Infrastructure")]
        public bool logSave = true;
        public bool logArt = true;
    }
}