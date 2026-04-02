using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElderBlood
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [SerializeField] private AssetReference _playerPrefab;
        [SerializeField] private WorldCreator _worldCreator;

        private GameObject _currentLevel;
        private GameObject _currentPlayer;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadLevel(AssetReference levelPrefabRef, Transform parent)
        {
            StartCoroutine(LoadLevelRoutine(levelPrefabRef, parent));
        }

        private IEnumerator LoadLevelRoutine(AssetReference levelPrefabRef, Transform parent)
        {
            UnloadCurrentLevel();

            var handle = Addressables.InstantiateAsync(levelPrefabRef, parent);
            yield return handle;

            _currentLevel = handle.Result;

            var bootstrap = _currentLevel.GetComponentInChildren<LevelBootstrap>();
            SpawnPlayer(bootstrap != null ? bootstrap.PlayerSpawnPoint : null);
            _worldCreator.CreateWorld();
        }

        private void UnloadCurrentLevel()
        {
            if (_currentPlayer != null)
            {
                Destroy(_currentPlayer);
                _currentPlayer = null;
            }

            if (_currentLevel != null)
            {
                Addressables.ReleaseInstance(_currentLevel);
                _currentLevel = null;
            }
        }

        private void SpawnPlayer(Transform spawnPoint)
        {
            var position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            var handle = Addressables.InstantiateAsync(_playerPrefab, position, Quaternion.identity);
            handle.Completed += h => _currentPlayer = h.Result;
        }
    }
}
