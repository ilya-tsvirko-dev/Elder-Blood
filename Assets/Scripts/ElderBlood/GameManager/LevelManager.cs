using System;
using System.Collections;
using ElderBlood.Configs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ElderBlood.Services;
using VContainer;

namespace ElderBlood
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private WorldCreator _worldCreator;
        [SerializeField] private Transform _levelsParent;

        public event Action<Transform> PlayerSpawned;

        private LevelContainer _levelContainer;
        private AssetReference _playerPrefab;
        private RunChannel _runChannel;

        private GameObject _currentLevel;
        private GameObject _currentPlayer;
        private int _currentIndex = -1;
        private bool _transitioning;
        private bool _runOver;
        private Transform _pendingSpawnPoint;

        [Inject]
        public void Construct(MainConfig config, RunChannel runChannel)
        {
            _levelContainer = config.LevelContainer;
            _playerPrefab = config.CharacterConfig.playerPrefab;
            _runChannel = runChannel;
        }

        // Begins a fresh run: one player for the whole run, floors teleport it to their spawn point.
        public void StartRun()
        {
            StopAllCoroutines();
            _transitioning = false;
            _runOver = false;
            _worldCreator.CreateWorld();
            _currentIndex = -1;
            GameLogger.Run("[Run] Start run");

            SpawnPlayer();
            LoadFloor(0);
        }

        // Called by ObjectiveSystem when the player reaches the exit.
        public void AdvanceFloor()
        {
            // ObjectiveSystem keeps polling proximity while the player stands on the exit, so both the
            // transition and the finished-run states have to swallow repeat calls.
            if (_transitioning || _runOver)
            {
                return;
            }

            var next = _currentIndex + 1;
            if (next < _levelContainer.Count)
            {
                LoadFloor(next);
            }
            else
            {
                WinRun();
            }
        }

        // Called by the run-end UI (restart button). Meta is already persisted via PlayerProgressSystem.
        public void RestartRun()
        {
            GameLogger.Run("[Run] Restart run (floor 1)");
            DestroyPlayer();
            StartRun();
        }

        // Called by ObjectiveSystem on player death — shows the run-end screen (no auto-restart).
        public void EndRunDeath()
        {
            if (_runOver)
            {
                return;
            }

            _runOver = true;
            GameLogger.Run("[Run] Player died → run lost");
            _runChannel.RaiseLost();
        }

        private void WinRun()
        {
            _runOver = true;
            GameLogger.Run("[Run] WIN — reached the top");
            _runChannel.RaiseWon();
        }

        private void LoadFloor(int index)
        {
            if (_transitioning)
            {
                return;
            }

            _transitioning = true;
            _currentIndex = index;
            GameLogger.Run($"[Run] Loading floor {index + 1}/{_levelContainer.Count}");
            StartCoroutine(LoadFloorRoutine(_levelContainer.GetByIndex(index)));
        }

        private IEnumerator LoadFloorRoutine(AssetReference levelPrefabRef)
        {
            UnloadCurrentLevel();

            var handle = Addressables.InstantiateAsync(levelPrefabRef, _levelsParent);
            yield return handle;

            _currentLevel = handle.Result;

            var bootstrap = _currentLevel.GetComponentInChildren<LevelBootstrap>();
            _pendingSpawnPoint = bootstrap != null ? bootstrap.PlayerSpawnPoint : null;
            TryPlacePlayer();

            _transitioning = false;
        }

        private void SpawnPlayer()
        {
            var handle = Addressables.InstantiateAsync(_playerPrefab, Vector3.zero, Quaternion.identity);
            handle.Completed += h =>
            {
                _currentPlayer = h.Result;
                PlayerSpawned?.Invoke(_currentPlayer.transform);
                TryPlacePlayer();
            };
        }

        // Places the persistent player at the current floor's spawn point. Safe to call from either
        // the level-load routine or the player-spawn callback (whichever finishes last wins).
        private void TryPlacePlayer()
        {
            if (_currentPlayer == null)
            {
                return;
            }

            _currentPlayer.transform.position = _pendingSpawnPoint != null
                ? _pendingSpawnPoint.position
                : Vector3.zero;
        }

        private void UnloadCurrentLevel()
        {
            if (_currentLevel != null)
            {
                Addressables.ReleaseInstance(_currentLevel);
                _currentLevel = null;
            }
        }

        private void DestroyPlayer()
        {
            if (_currentPlayer != null)
            {
                Destroy(_currentPlayer);
                _currentPlayer = null;
            }
        }
    }
}
