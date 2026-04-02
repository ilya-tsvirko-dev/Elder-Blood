using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElderBlood
{
    public class BootstrapEntryPoint : MonoBehaviour
    {
        [SerializeField] private string _mainSceneName = "Main";

        private void Start()
        {
            SceneManager.LoadScene(_mainSceneName);
        }
    }
}
