using System.Collections.Generic;
using Base.Const;
using Base.Events;
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
            if (entityManager.HasComponent<Block>(cube)) return cube;
            entityManager.AddComponent<Block>(cube);
            entityManager.AddComponent<Chunk>(cube);
            entityManager.AddComponent<GameWorld>(cube);
            entityManager.AddComponentData(cube,new RenderBounds { Value = SubMeshCacheManager.Instance.RenderEdge });
            return cube;
        }

        private void GenerateChunkBlocks(EntityManager entityManager, EntityCommandBuffer ecb, Entity prototype,
            ChunkUpdateEvent @event) {
            var chunk = @event.Chunk;
            if (chunk == null) return;
            // TODO 如何在检测到差异之后，用相对较低的代价来更新部分方块，而不是全部？
            var pos = new Vector3(
                chunk.Position.X,
                chunk.Position.Y,
                chunk.Position.Z
            );
            if (chunk.Version == LocalChunkManager.Instance.GetChunkVersion(pos)) return;
            var chunkExist = LocalChunkManager.Instance.GetChunkVersion(pos) == -1;
            if (!chunkExist && chunk.IsEmpty) return;
            if (chunkExist) {
                // 区块尚未生成
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
                                ChunkPos = pos,
                                WorldId = chunk.WorldId
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
            } else {
                // 区块已经生成，但是需要更新
                _query.SetSharedComponentFilterManaged(new Chunk {
                    Pos = pos
                });
                var entities = _query.ToEntityArray(Allocator.TempJob);
                var entityMap = new Dictionary<Vector3, Entity>();
                foreach (var entity in entities) {
                    var block = entityManager.GetComponentData<Block>(entity);
                    entityMap.Add(block.Pos, entity);
                }
                var job = new BlockUpdateJob {
                    Ecb = ecb.AsParallelWriter(),
                    EntityData = entities,
                    BlockEntities = entityMap,
                    Chunk = chunk
                };
                var task = job.Schedule(ParamConst.ChunkSize * ParamConst.ChunkSize * ParamConst.ChunkSize, 256);
                task.Complete();
                _query.ResetFilter();
                entities.Dispose();
                LocalChunkManager.Instance.AddChunkVersion(pos, chunk.Version);
            }
        } 
    }
}