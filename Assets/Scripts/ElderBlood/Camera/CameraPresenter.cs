using System;
using UnityEngine;

namespace ElderBlood
{
    public class CameraPresenter : IDisposable
    {
        private readonly CameraController _camera;
        private readonly LevelManager _levelManager;

        public CameraPresenter(CameraController camera, LevelManager levelManager)
        {
            _camera = camera;
            _levelManager = levelManager;

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
