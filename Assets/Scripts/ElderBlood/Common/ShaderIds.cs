using UnityEngine;

namespace ElderBlood.Common
{
    // Centralized shader property ids (CharactersHeals).
    public static class ShaderIds
    {
        public static readonly int Fill = Shader.PropertyToID("_FillAmount");
        public static readonly int Alpha = Shader.PropertyToID("_Alpha");
    }
}
