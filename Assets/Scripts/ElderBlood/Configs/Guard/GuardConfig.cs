using UnityEngine;

namespace ElderBlood.Configs.Guard
{
    [CreateAssetMenu(fileName = "GuardConfig", menuName = "Configs/GuardConfig", order = 1)]
    public class GuardConfig : ScriptableObject
    {
        public int MaxHealth;
    }
}