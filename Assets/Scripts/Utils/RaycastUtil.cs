using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Utils {
    /// <summary>
    /// 射线工具类
    /// </summary>
    public static class RaycastUtil {
        /// <summary>
        /// 射线检测，指定一条三维有向线段，返回碰撞到的最近的实体
        /// </summary>
        /// <param name="world">物理世界实例</param>
        /// <param name="rayFrom">线段起点</param>
        /// <param name="rayTo">线段终点</param>
        /// <returns>最近的实体</returns>
        public static Entity Raycast(PhysicsWorldSingleton world, float3 rayFrom, float3 rayTo) {
            var input = new RaycastInput {
                Start = rayFrom,
                End = rayTo,
                Filter = new CollisionFilter {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };
            var haveHit = world.CastRay(input, out var hit);
            return haveHit ? hit.Entity : Entity.Null;
        }
    }
}