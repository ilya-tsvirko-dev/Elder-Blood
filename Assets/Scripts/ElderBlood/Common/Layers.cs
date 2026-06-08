using UnityEngine;

namespace ElderBlood.Common
{
    // Centralized layer names and cached masks.
    public static class Layers
    {
        public const string CharacterName = "Character";

        public static readonly int CharacterMask = LayerMask.GetMask(CharacterName);
    }
}
