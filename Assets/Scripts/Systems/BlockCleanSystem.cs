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
        protected override void OnCreate() {
            Enabled = false;
        }

        [BurstCompile]
        protected override void OnUpdate() {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var chunks = new HashSet<Vector3>();
            // 查找区块
            foreach (var (player, _) in SystemAPI.Query<RefRO<Player>, RefRO<Self>>()) {
                var chunkList = LocalChunkManager.Instance.AutoUnloadChunk(player.ValueRO.Pos);
                foreach (var pos in chunkList) {
                    chunks.Add(pos);
                }
            }

            // 卸载区块
            var transformArray = new List<Entity>();
            foreach (var chunkPos in chunks) {
                Entities.WithSharedComponentFilter(new Chunk {
                    Pos = chunkPos
                }).ForEach((Entity entity) => transformArray.Add(entity)).WithoutBurst().Run();
            }
            var cubes = CollectionHelper.CreateNativeArray<Entity>(transformArray.Count,
                Allocator.TempJob);
            for (var index = 0; index < transformArray.Count; index++) {
                var entity = transformArray[index];
                cubes[index] = entity;
            }
            entityManager.AddComponent<Disabled>(cubes);
            cubes.Dispose();
            LocalChunkManager.Instance.RemoveChunks(chunks);
            
            // 清理区块
            entityManager.DestroyEntity(GetEntityQuery(typeof(Disabled)));
        }
    }
}