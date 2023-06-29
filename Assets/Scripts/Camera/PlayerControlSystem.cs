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
        private const int MaxControlDistance = 10;
        private EntityQuery _query;

        [BurstCompile]
        protected override void OnCreate() {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Player, Self>()
                .Build(entityManager);
        }

        [BurstCompile]
        protected override void OnDestroy() {
            _query.Dispose();
        }

        [BurstCompile]
        protected override void OnUpdate() {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var player = _query.GetSingleton<Player>();
            var target = RaycastUtil.Raycast(collisionWorld, player.Pos, player.Forward * MaxControlDistance);
            if (target == Entity.Null) {
                return;
            }
            // TODO 绘制眼前实体的信息
            // TODO 检测按键并进行交互
        }
    }
}