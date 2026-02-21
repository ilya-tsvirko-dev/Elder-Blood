using System.Collections.Generic;
using Scellecs.Morpeh.Providers;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Guard.GuardPatrol
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardPatrolProvider : MonoProvider<GuardPatrolComponent> 
    {
        [SerializeField] private List<Transform> points = new ();
        
        [Button]
        public void SetPoints()
        {
            var setPoints = new List<Vector3>();
            foreach (var point in points)
            {
                setPoints.Add(point.position);
            }

            GetData().points = setPoints;
        }
    }
}