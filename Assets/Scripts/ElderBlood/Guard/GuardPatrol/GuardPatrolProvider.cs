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

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var drawPoints = GetData().points;
            
            if (drawPoints == null || drawPoints.Count == 0) return;

            Gizmos.color = Color.cyan;
            for (int i = 0; i < drawPoints.Count; i++)
            { 
                Gizmos.DrawSphere(drawPoints[i], 0.15f);

                int next = (i + 1) % drawPoints.Count;
                
                Gizmos.DrawLine(drawPoints[i], drawPoints[next]);

                UnityEditor.Handles.Label(drawPoints[i] + Vector3.up * 0.25f, i.ToString());
            }
        }
#endif
    }
}