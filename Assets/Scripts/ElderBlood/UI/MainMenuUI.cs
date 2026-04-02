using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ElderBlood
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private AssetReference _firstLevel;
        [SerializeField] private Transform _parent;

        private void Awake()
        {
            _startButton.onClick.AddListener(OnStartClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
        }

        private void OnStartClicked()
        {
            LevelManager.Instance.LoadLevel(_firstLevel, _parent);
        }
    }
}
