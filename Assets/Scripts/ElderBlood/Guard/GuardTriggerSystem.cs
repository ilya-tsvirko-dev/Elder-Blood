using ElderBlood.Guard.GuardBehavior;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardTriggerSystem : ISystem 
    {
        public World World { get; set;}
    
        private Filter _filter;
        private Stash<GuardTriggerComponent> _triggerStash;
        private Stash<GuardBehaviorComponent> _behaviorStash;
        
        private Event<DebugTriggerEvent> _debugEvents;

        public void OnAwake() 
        {
            _filter = World.Filter.With<GuardTriggerComponent>().With<GuardBehaviorComponent>().Build();
            _triggerStash = World.GetStash<GuardTriggerComponent>();
            _behaviorStash = World.GetStash<GuardBehaviorComponent>();

            _debugEvents = World.GetEvent<DebugTriggerEvent>();
            _debugEvents.Subscribe(HandleDebugTriggerEvents);
            
            _debugEvents.NextFrame(new DebugTriggerEvent(true));
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                ref var triggerComponent = ref _triggerStash.Get(entity);
                ref var behaviorComponent = ref _behaviorStash.Get(entity);
                
                var collider2D = Physics2D.OverlapCircle(triggerComponent.target.position, triggerComponent.radius, 
                    LayerMask.GetMask("Character"));

                if (collider2D == null)
                {
                    behaviorComponent.targetTransform = null;
                    continue;
                }
                
                behaviorComponent.targetTransform = collider2D.transform;
            }
        }

        public void Dispose()
        {

        }

        private void HandleDebugTriggerEvents(FastList<DebugTriggerEvent> triggers)
        {
            foreach (var trigger in triggers)
            {
                SetLineRenderer(trigger.showDebugLineRenderer);
            }
        }

        private void SetLineRenderer(bool showDebugLineRenderer)
        {
            foreach (var entity in _filter)
            {
                ref GuardTriggerComponent triggerComponent = ref _triggerStash.Get(entity);
                
                if (showDebugLineRenderer)
                {
                    triggerComponent.lineRenderer.gameObject.SetActive(true);
                    triggerComponent.lineRenderer.useWorldSpace = false;
                    triggerComponent.lineRenderer.loop = true;
                    triggerComponent.lineRenderer.positionCount = 32;

                    Vector3[] points = new Vector3[32];
                    for (int i = 0; i < 32; i++)
                    {
                        float angle = 2f * Mathf.PI * i / 32;
                        points[i] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * triggerComponent.radius;
                        points[i].z = 0;
                    }
                
                    triggerComponent.lineRenderer.SetPositions(points);
                }
                else
                {
                    triggerComponent.lineRenderer.gameObject.SetActive(false);
                }
            }
        }
    }
}