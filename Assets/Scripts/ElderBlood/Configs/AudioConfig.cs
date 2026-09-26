using ElderBlood.Ecs.Audio;
using UnityEngine;

namespace ElderBlood.Configs
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "Configs/AudioConfig", order = 9)]
    public class AudioConfig : ScriptableObject
    {
        [Header("SFX")]
        public AudioClip damageClip;
        public AudioClip alertClip;
        public AudioClip abilityCastClip;
        public AudioClip drainClip;

        [Header("Playback")]
        [Tooltip("Shortest gap between two plays of the same sound. Continuous sources (guard DPS) " +
                 "would otherwise retrigger every frame.")]
        public float repeatCooldown = 0.3f;

        // Placeholder: AudioMixerGroup for volume routing. Assign when an AudioMixer is wired.
        [Header("Mixer (optional)")]
        public UnityEngine.Audio.AudioMixerGroup mixerGroup;

        public AudioClip GetClip(SfxType type)
        {
            return type switch
            {
                SfxType.Damage => damageClip,
                SfxType.Alert => alertClip,
                SfxType.AbilityCast => abilityCastClip,
                SfxType.Drain => drainClip,
                _ => null
            };
        }
    }
}
