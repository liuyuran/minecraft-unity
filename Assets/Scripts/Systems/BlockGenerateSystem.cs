using System.Collections.Generic;
using System.Threading;
using Base;
using Base.Const;
using Base.Manager;
using Base.Messages;
using Components;
using Managers;
using Systems.Jobs;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Entity = Unity.Entities.Entity;
using EntityManager = Unity.Entities.EntityManager;

namespace Systems {
    /// <summary>
    /// 从基础库的支持类中获取命令执行队列，然后执行
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct BlockGenerateSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EntityGenerator>();
            new Thread(() => { Game.Start(""); }).Start();
            Thread.Sleep(1000);
            CommandTransferManager.NetworkAdapter?.SendToServer(new PlayerJoinEvent {
                Nickname = "Kamoeth"
            });
        }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            var chunkQueue = new List<ChunkUpdateEvent>();
            while (CommandTransferManager.NetworkAdapter?.TryGetFromClient(out var message) ?? false) {
                if (message == null) return;
                switch (message) {
                    case ChunkUpdateEvent chunkUpdateEvent:
                        chunkQueue.Add(chunkUpdateEvent);
                        break;
                }
            }
            if (chunkQueue.Count == 0) return;
            // 下一行看似没有意义，但是必须在这里预先获取Instance对象，不然会出问题
            SubMeshCacheManager.Instance.GetMeshId("classic:air");
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            // 开始刷新区块
            var prototype = GetBlockPrototype(entityManager);
            var transformArray = new List<BlockGenerateJob.BlockInfoForJob>();
            foreach (var @event in chunkQueue) {
                var chunk = @event.Chunk;
                if (chunk == null) continue;
                // TODO 如何在检测到差异之后，用相对较低的代价来更新部分方块，而不是全部？
                var pos = new Vector3(
                    chunk.Position.X,
                    chunk.Position.Y,
                    chunk.Position.Z
                );
                if (chunk.Version == LocalChunkManager.Instance.GetChunkVersion(pos)) continue;
                for (var x = 0; x < ParamConst.ChunkSize; x++) {
                    for (var y = 0; y < ParamConst.ChunkSize; y++) {
                        for (var z = 0; z < ParamConst.ChunkSize; z++) {
                            var block = chunk.GetBlock(x, y, z);
                            if (block.Transparent) continue;
                            transformArray.Add(new BlockGenerateJob.BlockInfoForJob {
                                BlockId = SubMeshCacheManager.Instance.GetMeshId(block.ID),
                                Pos = new float3(
                                    x + pos.x * ParamConst.ChunkSize,
                                    y + pos.y * ParamConst.ChunkSize,
                                    z + pos.z * ParamConst.ChunkSize
                                ),
                                RenderFlags = block.RenderFlags,
                                ChunkPos = pos
                            });
                        }
                    }
                }
                LocalChunkManager.Instance.AddChunkVersion(pos, chunk.Version);
            }

            var cubes = CollectionHelper.CreateNativeArray<BlockGenerateJob.BlockInfoForJob>(transformArray.Count,
                Allocator.TempJob);
            for (var i = 0; i < transformArray.Count; i++) {
                cubes[i] = transformArray[i];
            }

            var job = new BlockGenerateJob {
                Prototype = prototype,
                Ecb = ecb.AsParallelWriter(),
                Data = cubes
            };
            var task = job.Schedule(cubes.Length, 256);
            task.Complete();
            ecb.Playback(entityManager);
            ecb.Dispose();
            cubes.Dispose();
        }

        private Entity GetBlockPrototype(EntityManager entityManager) {
            var generator = SystemAPI.GetSingleton<EntityGenerator>();
            var cube = generator.Cube;
            entityManager.AddComponent<Block>(cube);
            entityManager.AddComponent<Chunk>(cube);
            entityManager.AddComponentData(cube, new RenderBounds { Value = SubMeshCacheManager.Instance.RenderEdge });
            return cube;
        }
    }
}