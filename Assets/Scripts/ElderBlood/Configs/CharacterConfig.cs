using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElderBlood.Configs
{
    [CreateAssetMenu(fileName = "CharacterConfig", menuName = "Configs/CharacterConfig", order = 4)]
    public class CharacterConfig : ScriptableObject
    {
        public AssetReference playerPrefab;
    }
}
