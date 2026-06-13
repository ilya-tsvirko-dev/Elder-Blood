using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class HudView : MonoBehaviour
    {
        [Header("Blood")]
        [SerializeField] private Image _bloodFill;
        [SerializeField] private TMP_Text _bloodText;

        [Header("Ability cooldown overlays (fill = remaining)")]
        [SerializeField] private Image _invisibilityCooldown;
        [SerializeField] private Image _puddleCooldown;
        [SerializeField] private Image _bloodSightCooldown;

        public void SetBlood(float current, float capacity)
        {
            if (_bloodFill != null)
            {
                _bloodFill.fillAmount = capacity > 0f ? current / capacity : 0f;
            }

            if (_bloodText != null)
            {
                _bloodText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(capacity)}";
            }
        }

        public void SetCooldowns(in HudCooldowns cooldowns)
        {
            if (_invisibilityCooldown != null)
            {
                _invisibilityCooldown.fillAmount = cooldowns.invisibility;
            }

            if (_puddleCooldown != null)
            {
                _puddleCooldown.fillAmount = cooldowns.puddle;
            }

            if (_bloodSightCooldown != null)
            {
                _bloodSightCooldown.fillAmount = cooldowns.bloodSight;
            }
        }
    }
}
