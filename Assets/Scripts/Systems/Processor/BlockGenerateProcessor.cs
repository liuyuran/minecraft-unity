using System.Collections.Generic;
using Base.Const;
using Base.Messages;
using Components;
using Managers;
using Systems.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Entity = Unity.Entities.Entity;
using SystemAPI = Unity.Entities.SystemAPI;

namespace Systems.Processor {
    public partial struct ServerCommandExecSystem {
        private Entity GetBlockPrototype(EntityManager entityManager) {
            var generator = SystemAPI.GetSingleton<EntityGenerator>();
            var cube = generator.Cube;
            entityManager.AddComponent<Block>(cube);
            entityManager.AddComponent<Chunk>(cube);
            entityManager.AddComponentData(cube, new RenderBounds { Value = SubMeshCacheManager.Instance.RenderEdge });
            return cube;
        }

        private void GenerateChunkBlocks(EntityCommandBuffer ecb, Entity prototype, ChunkUpdateEvent @event) {
            var chunk = @event.Chunk;
            if (chunk == null) return;
            // TODO 如何在检测到差异之后，用相对较低的代价来更新部分方块，而不是全部？
            var pos = new Vector3(
                chunk.Position.X,
                chunk.Position.Y,
                chunk.Position.Z
            );
            if (chunk.Version == LocalChunkManager.Instance.GetChunkVersion(pos)) return;
            var transformArray = new List<BlockGenerateJob.BlockInfoForJob>();
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
            cubes.Dispose();
        } 
    }
}