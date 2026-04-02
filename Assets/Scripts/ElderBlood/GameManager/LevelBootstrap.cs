using UnityEngine;

namespace ElderBlood
{
    public class LevelBootstrap : MonoBehaviour
    {
        [field: SerializeField] public Transform PlayerSpawnPoint { get; private set; }
    }
}
