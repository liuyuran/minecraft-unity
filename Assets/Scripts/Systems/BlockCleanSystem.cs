using System.Collections.Generic;
using Camera;
using Components;
using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems {
    /// <summary>
    /// 清理区块
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct BlockCleanSystem : ISystem {
        private EntityQuery _query;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Disabled>()
                .Build(state.EntityManager);
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
            _query.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            // 先禁用，然后在下一逻辑帧清理
            var entityManager = state.EntityManager;
            // 清理此前禁用的区块
            entityManager.DestroyEntity(_query);
            var chunks = new HashSet<Vector3>();
            // 查找区块
            foreach (var (player, _) in SystemAPI.Query<RefRO<Player>, RefRO<Self>>()) {
                var chunkList = LocalChunkManager.Instance.AutoUnloadChunk(player.ValueRO.Pos);
                foreach (var pos in chunkList) {
                    chunks.Add(pos);
                }
            }
            // 卸载区块
            var query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Chunk>()
                .Build(state.EntityManager);
            foreach (var chunkPos in chunks) {
                query.ResetFilter();
                query.SetSharedComponentFilter(new Chunk {
                    Pos = chunkPos
                });
                entityManager.AddComponent<Disabled>(query);
            }
            LocalChunkManager.Instance.RemoveChunks(chunks);
        }
    }
}