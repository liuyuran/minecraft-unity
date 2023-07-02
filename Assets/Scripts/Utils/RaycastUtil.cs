using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Utils {
    /// <summary>
    /// 射线工具类
    /// </summary>
    public static class RaycastUtil {
        public struct RaycastResult {
            public Entity Entity;
            public Direction Direction;
        }

        /// <summary>
        /// 射线检测，指定一条三维有向线段，返回碰撞到的最近的实体
        /// TODO 待验证，只是理论上可行
        /// </summary>
        /// <param name="world">物理世界实例</param>
        /// <param name="rayFrom">线段起点</param>
        /// <param name="rayTo">线段终点</param>
        /// <returns>最近的实体，若不存在，返回null</returns>
        public static RaycastResult? Raycast(PhysicsWorldSingleton world, float3 rayFrom, float3 rayTo) {
            var input = new RaycastInput {
                Start = rayFrom,
                End = rayTo,
                Filter = new CollisionFilter {
                    BelongsTo = ~(1u << 0), // 这里的意思是排除0层的刚体，~的意思是按位取反，<<是二进制左移，其最终值应该是按二进制位读取的
                    CollidesWith = 1u << 0, // 这个参数和editor中同名参数含义类似，但这里的值实际测试中必须是BelongsTo的按位取反，不明白为什么
                    GroupIndex = 0 // 意义不明，暂且写0
                }
            };
            var haveHit = world.CastRay(input, out var hit);
            var direction = Direction.up;
            if (Vector3.Dot(hit.SurfaceNormal, Vector3.up) > 0.9f) {
                direction = Direction.up;
            } else if (Vector3.Dot(hit.SurfaceNormal, Vector3.down) > 0.9f) {
                direction = Direction.down;
            } else if (Vector3.Dot(hit.SurfaceNormal, Vector3.left) > 0.9f) {
                direction = Direction.west;
            } else if (Vector3.Dot(hit.SurfaceNormal, Vector3.right) > 0.9f) {
                direction = Direction.east;
            } else if (Vector3.Dot(hit.SurfaceNormal, Vector3.forward) > 0.9f) {
                direction = Direction.south;
            } else if (Vector3.Dot(hit.SurfaceNormal, Vector3.back) > 0.9f) {
                direction = Direction.north;
            }

            return haveHit ?
                new RaycastResult {
                    Entity = hit.Entity,
                    Direction = direction
                } :
                null;
        }
    }
}