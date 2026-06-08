using System;
using UnityEngine;
using VContainer.Unity;

namespace ElderBlood
{
    public class CameraPresenter : IStartable, IDisposable
    {
        private readonly CameraController _camera;
        private readonly LevelManager _levelManager;

        public CameraPresenter(CameraController camera, LevelManager levelManager)
        {
            _camera = camera;
            _levelManager = levelManager;
        }

        public void Start()
        {
            _levelManager.PlayerSpawned += OnPlayerSpawned;
        }

        public void Dispose()
        {
            _levelManager.PlayerSpawned -= OnPlayerSpawned;
        }

        private void OnPlayerSpawned(Transform player)
        {
            _camera.SetTarget(player);
        }
    }
}
