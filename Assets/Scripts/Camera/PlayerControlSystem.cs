using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Utils;

namespace Camera {
    /// <summary>
    /// 角色和世界交互
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial class PlayerControlSystem : SystemBase {
        private const int MaxControlDistance = 30;
        private EntityQuery _query;

        [BurstCompile]
        protected override void OnCreate() {
            RequireForUpdate<Player>();
            RequireForUpdate<Self>();
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Player, Self>()
                .Build(entityManager);
        }

        [BurstCompile]
        protected override void OnDestroy() {
            _query.Dispose();
        }

        private void AttackAction(PhysicsWorldSingleton collisionWorld) {
            var transform = CameraLink.Instance.transform;
            var forward = transform.forward;
            var target = RaycastUtil.Raycast(collisionWorld, transform.position + forward * 5, forward * MaxControlDistance);
            if (target == Entity.Null) {
                return;
            }
            var position = EntityManager.GetComponentData<LocalTransform>(target).Position;
            Debug.Log($"{position.x}, {position.y}, {position.z}");
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var player = _query.GetSingleton<Player>();
            if (InputManager.Instance.CurrentPlan.Attack.WasPressedThisFrame()) {
                AttackAction(collisionWorld);
            }
            // TODO 绘制眼前实体的信息
            // TODO 检测按键并进行交互
        }
    }
}