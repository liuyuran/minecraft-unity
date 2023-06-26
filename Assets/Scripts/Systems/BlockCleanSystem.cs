using System.Collections.Generic;
using Camera;
using Components;
using Managers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Systems {
    /// <summary>
    /// 清理区块
    /// </summary>
    [BurstCompile]
    public partial class BlockCleanSystem : SystemBase {
        [BurstCompile]
        protected override void OnUpdate() {
            // 下一行看似没有意义，但是必须在这里预先获取Instance对象，不然会出问题
            SubMeshCacheManager.Instance.GetMeshId("classic:air");
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var chunks = new HashSet<Vector3>();
            // 查找区块
            foreach (var (player, _) in SystemAPI.Query<RefRO<Player>, RefRO<Self>>()) {
                chunks.AddRange(LocalChunkManager.Instance.AutoUnloadChunk(player.ValueRO.Pos));
            }

            // 卸载区块
            var transformArray = new List<Entity>();
            foreach (var chunkPos in chunks) {
                Entities.WithSharedComponentFilter(new Chunk {
                    Pos = chunkPos
                }).ForEach((Entity entity) => transformArray.Add(entity)).WithoutBurst().Run();
            }

            foreach (var entity in transformArray) {
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}