using System.Collections.Generic;
using Scellecs.Morpeh.Providers;
using Sirenix.OdinInspector;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
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
        // Edit-mode preview only (point placement). Runtime path drawing lives in GuardPatrolSystem.
        private void OnDrawGizmosSelected()
        {
            if (points == null || points.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(points[i].position, 0.15f);
                UnityEditor.Handles.Label(points[i].position + Vector3.up * 0.25f, i.ToString());

                var next = points[(i + 1) % points.Count];
                if (next != null)
                {
                    Gizmos.DrawLine(points[i].position, next.position);
                }
            }
        }
#endif
    }
}