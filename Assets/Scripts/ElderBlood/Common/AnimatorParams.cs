using UnityEngine;

namespace ElderBlood.Common
{
    // Centralized Animator parameter names and their cached hashes.
    // Names double as animation clip name prefixes (e.g. "Jump" → "JumpDownSide").
    public static class AnimatorParams
    {
        public const string IdleName = "Idle";
        public const string MoveName = "Move";
        public const string RunName = "Run";
        public const string PuddleName = "Puddle";
        public const string WakeUpName = "WakeUp";
        public const string JumpName = "Jump";
        public const string DeathName = "Death";
        public const string MagicName = "Magic";
        public const string HideName = "Hide";
        public const string AttackName = "Attack";
        public const string XDirectionName = "XDirection";
        public const string YDirectionName = "YDirection";

        public static readonly int Idle = Animator.StringToHash(IdleName);
        public static readonly int Move = Animator.StringToHash(MoveName);
        public static readonly int Run = Animator.StringToHash(RunName);
        public static readonly int Puddle = Animator.StringToHash(PuddleName);
        public static readonly int WakeUp = Animator.StringToHash(WakeUpName);
        public static readonly int Jump = Animator.StringToHash(JumpName);
        public static readonly int Death = Animator.StringToHash(DeathName);
        public static readonly int Magic = Animator.StringToHash(MagicName);
        public static readonly int Hide = Animator.StringToHash(HideName);
        public static readonly int Attack = Animator.StringToHash(AttackName);
        public static readonly int XDirection = Animator.StringToHash(XDirectionName);
        public static readonly int YDirection = Animator.StringToHash(YDirectionName);
    }
}
