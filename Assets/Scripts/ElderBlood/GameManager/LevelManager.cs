using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ElderBlood
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelContainer _levelContainer;
        [SerializeField] private AssetReference _playerPrefab;
        [SerializeField] private WorldCreator _worldCreator;
        [SerializeField] private Transform _levelsParent;

        public event Action<Transform> PlayerSpawned;

        private GameObject _currentLevel;
        private GameObject _currentPlayer;
        private int _currentIndex = -1;

        public void LoadFirstLevel()
        {
            LoadLevel(0);
        }

        public void LoadNextLevel()
        {
            var nextIndex = _currentIndex + 1;
            if (nextIndex < _levelContainer.Count)
                LoadLevel(nextIndex);
        }

        public void LoadLevel(int index)
        {
            _currentIndex = index;
            StartCoroutine(LoadLevelRoutine(_levelContainer.GetByIndex(index)));
        }

        private IEnumerator LoadLevelRoutine(AssetReference levelPrefabRef)
        {
            UnloadCurrentLevel();

            var handle = Addressables.InstantiateAsync(levelPrefabRef, _levelsParent);
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
            handle.Completed += h =>
            {
                _currentPlayer = h.Result;
                PlayerSpawned?.Invoke(_currentPlayer.transform);
            };
        }
    }
}
