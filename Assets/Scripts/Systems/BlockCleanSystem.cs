using System.Collections.Generic;
using Camera;
using Components;
using Managers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var chunks = new HashSet<Vector3>();
            // 查找区块
            foreach (var (player, _) in SystemAPI.Query<RefRO<Player>, RefRO<Self>>()) {
                var chunkList = LocalChunkManager.Instance.AutoUnloadChunk(player.ValueRO.Pos);
                foreach (var pos in chunkList) {
                    chunks.Add(pos);
                    if (chunks.Count > 1) break;
                }
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
            LocalChunkManager.Instance.RemoveChunks(chunks);
        }
    }
}