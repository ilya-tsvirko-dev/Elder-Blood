using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ElderBlood
{
    public class HudView : MonoBehaviour
    {
        // An ability icon is drawn twice: a drained copy underneath, and the full-colour copy
        // on top clipped from the bottom. As the cooldown runs out the colour "fills up" with
        // blood. No numbers, and the icon silhouette is never covered by an overlay.
        [Header("Blood")]
        [SerializeField] private Image _bloodFill;
        [SerializeField] private TMP_Text _bloodText;

        [Header("Shield")]
        [SerializeField] private Image _shieldFill;
        [SerializeField] private TMP_Text _shieldText;

        [Header("Blood Sight vignette")]
        [Tooltip("Full-screen overlay shown while Blood Sight is active; it fades out as the ability runs down.")]
        [SerializeField] private Image _bloodSightOverlay;
        [Tooltip("Alpha of the overlay at full strength, right after the cast.")]
        [Range(0f, 1f)]
        [SerializeField] private float _bloodSightPeakAlpha = 0.15f;

        [Header("Invisibility icon")]
        [SerializeField] private Image _invisibilityDrained;
        [SerializeField] private Image _invisibilityFill;

        [Header("Blood puddle icon")]
        [SerializeField] private Image _puddleDrained;
        [SerializeField] private Image _puddleFill;

        [Header("Blood sight icon")]
        [SerializeField] private Image _bloodSightDrained;
        [SerializeField] private Image _bloodSightFill;

        [Header("Icon tints")]
        [Tooltip("Under-layer: the ability drained of blood.")]
        [SerializeField] private Color _drainedTint = new Color(0.22f, 0.05f, 0.07f, 1f);
        [Tooltip("Filled and castable - leave white to show the sprite's own colours.")]
        [SerializeField] private Color _readyTint = Color.white;
        [Tooltip("Off cooldown but there is not enough blood to pay the cost.")]
        [SerializeField] private Color _unaffordableTint = new Color(0.42f, 0.31f, 0.34f, 1f);

        private void Awake()
        {
            Configure(_invisibilityFill);
            Configure(_puddleFill);
            Configure(_bloodSightFill);
        }

        public void SetBlood(float current, float capacity)
        {
            SetBar(_bloodFill, _bloodText, current, capacity);
        }

        // The shield bar hides entirely until the upgrade is bought, rather than sitting at zero.
        public void SetShield(float current, float max, bool unlocked)
        {
            if (_shieldFill != null)
            {
                _shieldFill.gameObject.SetActive(unlocked);
            }

            if (_shieldText != null)
            {
                _shieldText.gameObject.SetActive(unlocked);
            }

            if (unlocked)
            {
                SetBar(_shieldFill, _shieldText, current, max);
            }
        }

        private static void SetBar(Image fill, TMP_Text text, float current, float max)
        {
            if (fill != null)
            {
                fill.fillAmount = max > 0f ? current / max : 0f;
            }

            if (text != null)
            {
                text.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
            }
        }

        // `remaining` is the 0..1 share of the duration still left, already quantised by HudFeedSystem —
        // writing a Graphic's colour dirties the whole canvas, so this must not run every frame.
        public void SetBloodSight(bool active, float remaining)
        {
            if (_bloodSightOverlay == null)
            {
                return;
            }

            if (_bloodSightOverlay.gameObject.activeSelf != active)
            {
                _bloodSightOverlay.gameObject.SetActive(active);
            }

            if (active)
            {
                var c = _bloodSightOverlay.color;
                c.a = remaining * _bloodSightPeakAlpha;
                _bloodSightOverlay.color = c;
            }
        }

        public void SetAbilities(in HudAbilities abilities)
        {
            Apply(_invisibilityDrained, _invisibilityFill, abilities.invisibility);
            Apply(_puddleDrained, _puddleFill, abilities.puddle);
            Apply(_bloodSightDrained, _bloodSightFill, abilities.bloodSight);
        }

        // Set up the fill mode here rather than trusting the inspector - it is the one thing
        // that silently breaks the whole effect if it is wrong.
        private static void Configure(Image fill)
        {
            if (fill == null)
            {
                return;
            }

            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Vertical;
            fill.fillOrigin = (int)Image.OriginVertical.Bottom;
        }

        private void Apply(Image drained, Image fill, in HudAbilityState state)
        {
            if (drained != null)
            {
                drained.color = _drainedTint;
            }

            if (fill != null)
            {
                fill.fillAmount = state.readiness;
                fill.color = state.affordable ? _readyTint : _unaffordableTint;
            }
        }
    }
}
