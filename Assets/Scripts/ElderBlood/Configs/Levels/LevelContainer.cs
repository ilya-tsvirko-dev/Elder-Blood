using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElderBlood
{
    [CreateAssetMenu(fileName = "LevelContainer", menuName = "Configs/LevelContainer", order = 2)]
    public class LevelContainer : ScriptableObject
    {
        [SerializeField] private List<AssetReference> _levels;

        public int Count => _levels.Count;

        public AssetReference First => _levels[0];

        public AssetReference GetByIndex(int index) => _levels[index];
    }
}
