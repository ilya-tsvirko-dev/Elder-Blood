using UnityEngine;

namespace ElderBlood
{
    public class MainEntryPoint : MonoBehaviour
    {
        [SerializeField] private MainMenuView _view;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private CameraController _camera;

        private MainMenuPresenter _menuPresenter;
        private CameraPresenter _cameraPresenter;

        private void Awake()
        {
            _menuPresenter = new MainMenuPresenter(_view, _levelManager);
            _cameraPresenter = new CameraPresenter(_camera, _levelManager);
        }

        private void OnDestroy()
        {
            _menuPresenter.Dispose();
            _cameraPresenter.Dispose();
        }
    }
}
