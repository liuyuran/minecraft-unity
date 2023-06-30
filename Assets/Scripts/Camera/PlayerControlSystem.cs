using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
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
        
        /// <summary>
        /// 获取射线碰撞的目标
        /// </summary>
        /// <param name="collisionWorld">物理世界</param>
        /// <returns>屏幕中心指向的目标</returns>
        private RaycastUtil.RaycastResult? GetRaycastTarget(PhysicsWorldSingleton collisionWorld) {
            var transform = CameraLink.Instance.transform;
            var forward = transform.forward;
            // TODO 感觉这里需要优化为从屏幕中心发射射线，而不是直接读取摄像头的朝向
            return RaycastUtil.Raycast(collisionWorld, transform.position + forward * 5, forward * MaxControlDistance);
        }

        /// <summary>
        /// 攻击、挖掘方块
        /// </summary>
        /// <param name="collisionWorld">物理世界实例</param>
        private void AttackAction(PhysicsWorldSingleton collisionWorld) {
            var target = GetRaycastTarget(collisionWorld);
            if (target == null) return;
        }
        
        /// <summary>
        /// 防御、放置方块
        /// </summary>
        /// <param name="collisionWorld"></param>
        private void DefenceAction(PhysicsWorldSingleton collisionWorld) {
            var target = GetRaycastTarget(collisionWorld);
            if (target == null) return;
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