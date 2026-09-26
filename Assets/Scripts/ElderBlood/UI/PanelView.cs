using UnityEngine;

namespace ElderBlood
{
    // Base for screens built as a child panel under a host object that stays active: the host keeps
    // the MonoBehaviour alive (and resolvable by VContainer) while the panel itself is switched off.
    public abstract class PanelView : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        public virtual void Show()
        {
            // The host may be disabled in the scene; activating only the child panel would do nothing.
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (_panel != null)
            {
                _panel.SetActive(true);
            }
        }

        public virtual void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        // Derived views wire their buttons here; the panel starts hidden either way.
        protected virtual void Awake()
        {
            Hide();
        }
    }
}
