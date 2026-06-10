using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Progression
{
    // Vendor point. Player buys upgrades while within interactRadius of it.
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct GodOfBloodComponent : IComponent
    {
        public Transform transform;
        public float interactRadius;
    }
}
