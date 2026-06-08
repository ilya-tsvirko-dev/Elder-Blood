using ElderBlood.Common;
using ElderBlood.Ecs.Movement;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace ElderBlood.Ecs.Guard
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class GuardVisionSystem : ISystem
    {
        public World World { get; set; }

        private Filter _filter;
        private Stash<GuardVisionComponent> _visionStash;
        private Stash<GuardPerceptionComponent> _perceptionStash;
        private Stash<GuardBehaviorComponent> _behaviorStash;
        private Stash<MoveComponent> _moveStash;

        public void OnAwake()
        {
            _filter = World.Filter.With<GuardVisionComponent>().With<GuardPerceptionComponent>()
                .With<GuardBehaviorComponent>().With<MoveComponent>().Build();
            _visionStash = World.GetStash<GuardVisionComponent>();
            _perceptionStash = World.GetStash<GuardPerceptionComponent>();
            _behaviorStash = World.GetStash<GuardBehaviorComponent>();
            _moveStash = World.GetStash<MoveComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var entity in _filter)
            {
                ref var vision = ref _visionStash.Get(entity);
                ref var perception = ref _perceptionStash.Get(entity);
                ref var behavior = ref _behaviorStash.Get(entity);
                ref var move = ref _moveStash.Get(entity);

                // Facing follows movement and holds the last direction when idle.
                if (move.moveVector.sqrMagnitude > 0.0001f)
                {
                    vision.facing = ((Vector2)move.moveVector).normalized;
                }

                var origin = (Vector2)behavior.transform.position + vision.eyeOffset;

                Transform seen = null;
                var visible = false;

                var candidate = Physics2D.OverlapCircle(origin, vision.viewRadius, Layers.CharacterMask);
                if (candidate != null)
                {
                    Vector2 toTarget = (Vector2)candidate.transform.position - origin;
                    var dist = toTarget.magnitude;

                    if (dist > 0.0001f && dist <= vision.viewRadius
                        && Vector2.Angle(vision.facing, toTarget) <= vision.viewAngle * 0.5f)
                    {
                        var blocker = Physics2D.Raycast(origin, toTarget / dist, dist, vision.obstacleMask);
                        if (blocker.collider == null)
                        {
                            visible = true;
                            seen = candidate.transform;
                            perception.lastSeenPosition = candidate.transform.position;

                            var proximity = 1f - Mathf.Clamp01(dist / vision.viewRadius);
                            perception.awareness += vision.fillSpeed * proximity * deltaTime;
                        }
                    }
                }

                if (!visible)
                {
                    perception.awareness -= vision.decaySpeed * deltaTime;
                }

                perception.awareness = Mathf.Clamp01(perception.awareness);

                if (perception.awareness >= 1f)
                {
                    perception.state = PerceptionState.Alert;
                }
                else if (perception.awareness > 0f)
                {
                    perception.state = PerceptionState.Suspicious;
                }
                else
                {
                    perception.state = PerceptionState.Unaware;
                }

                behavior.targetTransform = perception.state == PerceptionState.Alert ? seen : null;

                DrawDebug(origin, vision, perception.state);
            }
        }

        public void Dispose()
        {
        }

        private static void DrawDebug(Vector2 origin, GuardVisionComponent vision, PerceptionState state)
        {
            var color = state switch
            {
                PerceptionState.Alert => Color.red,
                PerceptionState.Suspicious => Color.yellow,
                _ => Color.green
            };

            var half = vision.viewAngle * 0.5f;
            Debug.DrawRay(origin, Rotate(vision.facing, half) * vision.viewRadius, color);
            Debug.DrawRay(origin, Rotate(vision.facing, -half) * vision.viewRadius, color);
            Debug.DrawRay(origin, vision.facing * vision.viewRadius, color);
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
