using Base.Events;
using Base.Manager;
using Components;
using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using Utils;
using Block = Components.Block;

namespace Camera {
    /// <summary>
    /// 角色和世界交互
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial class PlayerControlSystem : SystemBase {
        private const int MaxControlDistance = 4;
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
        
        /// <summary>
        /// 获取射线碰撞的目标
        /// </summary>
        /// <param name="collisionWorld">物理世界</param>
        /// <returns>屏幕中心指向的目标</returns>
        private RaycastUtil.RaycastResult? GetRaycastTarget(PhysicsWorldSingleton collisionWorld) {
            var center = new Vector3(Screen.width / 2, Screen.height / 2, 1);
            var pointA = CameraLink.Instance.ScreenToWorldPoint(center);
            center.z = MaxControlDistance;
            var pointB = CameraLink.Instance.ScreenToWorldPoint(center) - pointA;
            return RaycastUtil.Raycast(collisionWorld, pointA - pointB * 0.5f, pointB.normalized * MaxControlDistance + pointA);
        }

        /// <summary>
        /// 攻击、挖掘方块
        /// </summary>
        /// <param name="collisionWorld">物理世界实例</param>
        private void AttackAction(PhysicsWorldSingleton collisionWorld) {
            var target = GetRaycastTarget(collisionWorld);
            if (target == null) return;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var chunkPos = entityManager.GetSharedComponent<Chunk>(target.Value.Entity).Pos;
            var blockPos = entityManager.GetComponentData<Block>(target.Value.Entity).Pos;
            CommandTransferManager.NetworkAdapter?.SendToServer(new BlockUpdateEvent {
                ChunkPos = new System.Numerics.Vector3(chunkPos.x, chunkPos.y, chunkPos.z),
                BlockPos = new System.Numerics.Vector3(blockPos.x, blockPos.y, blockPos.z),
                ActionType = BlockUpdateEvent.ActionTypeEnum.Dig,
                Direction = target.Value.Direction
            });
        }
        
        /// <summary>
        /// 防御、放置方块
        /// </summary>
        /// <param name="collisionWorld"></param>
        private void DefenceAction(PhysicsWorldSingleton collisionWorld) {
            var target = GetRaycastTarget(collisionWorld);
            if (target == null) return;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var chunkPos = entityManager.GetSharedComponent<Chunk>(target.Value.Entity).Pos;
            var blockPos = entityManager.GetComponentData<Block>(target.Value.Entity).Pos;
            CommandTransferManager.NetworkAdapter?.SendToServer(new BlockUpdateEvent {
                ChunkPos = new System.Numerics.Vector3(chunkPos.x, chunkPos.y, chunkPos.z),
                BlockPos = new System.Numerics.Vector3(blockPos.x, blockPos.y, blockPos.z),
                ActionType = BlockUpdateEvent.ActionTypeEnum.Action,
                Direction = target.Value.Direction
            });
        }
        
        [BurstCompile]
        protected override void OnUpdate() {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            if (InputManager.Instance.CurrentPlan.Attack.WasPressedThisFrame()) {
                AttackAction(collisionWorld);
            }
            if (InputManager.Instance.CurrentPlan.Defence.WasPressedThisFrame()) {
                DefenceAction(collisionWorld);
            }
        }
    }
}