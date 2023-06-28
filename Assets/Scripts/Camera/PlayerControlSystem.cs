using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Camera {
    /// <summary>
    /// 角色和世界交互
    /// </summary>
    [BurstCompile]
    public partial class PlayerControlSystem : SystemBase {
        private KeyActionSettings _keyActionSettings;
        private KeyActionSettings.StandardActions _standardActions;

        protected override void OnCreate() {
            _keyActionSettings = new KeyActionSettings();
            _keyActionSettings.Enable();
            _standardActions = _keyActionSettings.standard;
        }

        protected override void OnDestroy() {
            _keyActionSettings.Dispose();
        }

        /// <summary>
        /// 射线检测，指定一条三维有向线段，返回碰撞到的最近的实体
        /// </summary>
        /// <param name="rayFrom">线段起点</param>
        /// <param name="rayTo">线段终点</param>
        /// <returns>最近的实体</returns>
        public Entity Raycast(float3 rayFrom, float3 rayTo) {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var input = new RaycastInput {
                Start = rayFrom,
                End = rayTo,
                Filter = new CollisionFilter {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };
            var haveHit = collisionWorld.CastRay(input, out var hit);
            return haveHit ? hit.Entity : Entity.Null;
        }

        [BurstCompile]
        protected override void OnUpdate() { }
    }
}