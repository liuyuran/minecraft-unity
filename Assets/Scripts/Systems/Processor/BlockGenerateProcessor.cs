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
                GenerateNewChunk(chunk, pos, ecb, prototype);
            } else {
                // 区块已经生成，但是需要更新
                ModifiedExistChunk(entityManager, chunk, pos, ecb, prototype);
            }
        }

        private void GenerateNewChunk(Base.Utils.Chunk chunk, Vector3 pos, EntityCommandBuffer ecb, Entity prototype) {
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
        }
        
        private bool IsEqual(Block clientBlock, Base.Blocks.Block serverBlock) {
            return SubMeshCacheManager.Instance.GetMeshId(serverBlock.ID) == clientBlock.BlockId;
        }

        private void ModifiedExistChunk(EntityManager entityManager, Base.Utils.Chunk chunk, Vector3 pos, EntityCommandBuffer ecb, Entity prototype) {
            _query.SetSharedComponentFilterManaged(new Chunk {
                Pos = pos
            });
            var entities = _query.ToEntityArray(Allocator.TempJob);
            var modified = new List<BlockUpdateJob.BlockInfoForJob>();
            var pointSet = new HashSet<Vector3>();
            // 先看有多少修改了的方块
            foreach (var item in entities) {
                var block = entityManager.GetComponentData<Block>(item);
                var serverBlock = chunk.GetBlock((int)block.Pos.x, (int)block.Pos.y, (int)block.Pos.z);
                if (IsEqual(block, serverBlock)) continue;
                modified.Add(new BlockUpdateJob.BlockInfoForJob {
                    ShouldCreate = false,
                    Entity = item,
                    BlockId = SubMeshCacheManager.Instance.GetMeshId(serverBlock.ID),
                    Pos = block.Pos,
                    RenderFlags = serverBlock.RenderFlags,
                    ChunkPos = pos,
                    WorldId = chunk.WorldId
                });
                pointSet.Add(block.Pos);
            }
            // 再看有多少新增的方块
            for (var x = 0; x < ParamConst.ChunkSize; x++) {
                for (var y = 0; y < ParamConst.ChunkSize; y++) {
                    for (var z = 0; z < ParamConst.ChunkSize; z++) {
                        if (pointSet.Contains(new Vector3(x, y, z))) continue;
                        var block = chunk.GetBlock(x, y, z);
                        if (block.Transparent) continue;
                        modified.Add(new BlockUpdateJob.BlockInfoForJob {
                            ShouldCreate = true,
                            Entity = null,
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
            var cubes = CollectionHelper.CreateNativeArray<BlockUpdateJob.BlockInfoForJob>(modified.Count,
                Allocator.TempJob);
            for (var i = 0; i < modified.Count; i++) {
                cubes[i] = modified[i];
            }
            var job = new BlockUpdateJob {
                Ecb = ecb.AsParallelWriter(),
                EntityData = cubes,
                Prototype = prototype
            };
            var task = job.Schedule(cubes.Length, 256);
            task.Complete();
            _query.ResetFilter();
            entities.Dispose();
            LocalChunkManager.Instance.AddChunkVersion(pos, chunk.Version);
        }
    }
}