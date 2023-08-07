using System.Collections.Generic;
using System.Linq;
using Base.Const;
using Base.Events.ServerEvent;
using Components;
using Managers;
using Systems.PlayingSystem.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using SystemAPI = Unity.Entities.SystemAPI;

namespace Systems.PlayingSystem.Processor {
    public partial struct ServerCommandExecSystem {
        private void GetBlockPrototype(EntityManager entityManager) {
            var generator = SystemAPI.GetSingleton<EntityGenerator>();
            var cube = generator.Cube;
            if (entityManager.HasComponent<Block>(cube)) return;
            entityManager.AddComponent<Block>(cube);
            entityManager.AddComponent<Chunk>(cube);
            entityManager.AddComponent<BlockTransform>(cube);
            entityManager.AddComponent<GameWorld>(cube);
            entityManager.AddComponentData(cube, new RenderBounds { Value = SubMeshCacheManager.Instance.RenderEdge });
        }
        
        private void GetItemPrototype(EntityManager entityManager) {
            var generator = SystemAPI.GetSingleton<EntityGenerator>();
            var cube = generator.Cube;
            if (entityManager.HasComponent<Item>(cube)) return;
            entityManager.AddComponent<Item>(cube);
            entityManager.AddComponent<Chunk>(cube);
            entityManager.AddComponent<BlockTransform>(cube);
            entityManager.AddComponent<GameWorld>(cube);
            entityManager.AddComponentData(cube, new RenderBounds { Value = SubMeshCacheManager.Instance.RenderEdge });
        }

        private void GenerateChunkBlocks(EntityGenerator generator, EntityCommandBuffer ecb, ChunkUpdateEvent @event) {
            var chunk = @event.Chunk;
            if (chunk == null) return;
            var pos = new Vector3(
                chunk.Position.X,
                chunk.Position.Y,
                chunk.Position.Z
            );
            if (chunk.Version == LocalChunkManager.Instance.GetChunkVersion(pos)) return;
            var chunkExist = LocalChunkManager.Instance.GetChunkVersion(pos) == -1;
            switch (chunkExist) {
                case true:
                    // 区块尚未生成
                    GenerateNewChunk(@event, pos, ecb, generator);
                    LocalChunkManager.Instance.AddChunkVersion(pos, chunk);
                    break;
                default:
                    // 区块已经生成，但是需要更新
                    ModifiedExistChunk(@event, pos, ecb, generator);
                    LocalChunkManager.Instance.AddChunkVersion(pos, chunk);
                    break;
            }
            UpdateItems(@event, pos, ecb, generator);
        }

        private void GenerateNewChunk(ChunkUpdateEvent @event, Vector3 pos, EntityCommandBuffer ecb, EntityGenerator generator) {
            var prototype = generator.Cube;
            var chunk = @event.Chunk;
            if (chunk == null) return;
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

            var cubes = CollectionHelper.CreateNativeArray<BlockGenerateJob.BlockInfoForJob>(transformArray.Count, Allocator.TempJob);
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

        private void ModifiedExistChunk(ChunkUpdateEvent @event, Vector3 pos, EntityCommandBuffer ecb, EntityGenerator generator) {
            var prototype = generator.Cube;
            var chunk = @event.Chunk;
            if (chunk == null) return;
            var oldChunk = LocalChunkManager.Instance.GetChunk(pos);
            var modified = new List<BlockUpdateJob.BlockInfoForJob>();
            // 先看有多少修改了的方块
            for (var i = 0; i < oldChunk.BlockData.Length; i++) {
                var serverBlock = chunk.BlockData[i];
                if (IsEqual(oldChunk.BlockData[i], serverBlock)) continue;
                var position = new Vector3(
                    i / ParamConst.ChunkSize % ParamConst.ChunkSize,
                    i / ParamConst.ChunkSize / ParamConst.ChunkSize,
                    i % ParamConst.ChunkSize
                );
                _blockQuery.SetSharedComponentFilter(new BlockTransform {
                    ChunkPos = new Vector3(
                        chunk.Position.X,
                        chunk.Position.Y,
                        chunk.Position.Z
                    ),
                    BlockPos = position
                });
                var oldIsAir = oldChunk.BlockData[i].Transparent;
                modified.Add(new BlockUpdateJob.BlockInfoForJob {
                    ShouldCreate = false,
                    Entity = oldIsAir ? null : _blockQuery.GetSingletonEntity(),
                    BlockId = SubMeshCacheManager.Instance.GetMeshId(serverBlock.ID),
                    Pos = position,
                    RenderFlags = serverBlock.RenderFlags,
                    ChunkPos = pos,
                    WorldId = chunk.WorldId
                });
            }

            var cubes = CollectionHelper.CreateNativeArray<BlockUpdateJob.BlockInfoForJob>(modified.Count, Allocator.TempJob);
            for (var i = 0; i < modified.Count; i++) {
                cubes[i] = modified[i];
            }

            var job = new BlockUpdateJob {
                Ecb = ecb.AsParallelWriter(),
                Data = cubes,
                Prototype = prototype
            };
            var task = job.Schedule(cubes.Length, 256);
            task.Complete();
            _blockQuery.ResetFilter();
        }

        private void UpdateItems(ChunkUpdateEvent @event, Vector3 pos, EntityCommandBuffer ecb, EntityGenerator generator) {
            var prototype = generator.Cube;
            var chunk = @event.Chunk;
            if (chunk == null) return;
            var serverItems = @event.Items;
            var localItems = LocalChunkManager.Instance.GetItem(pos);
            if (localItems == null) {
                // 本地没有任何物品
                if (serverItems.Count == 0) return;
                // 服务器有物品，本地没有物品
                var items = new List<ItemGenerateJob.ItemInfoForJob>();
                foreach (var (key, value) in serverItems) {
                    items.AddRange(value.Select(item => new ItemGenerateJob.ItemInfoForJob {
                        IsCreate = true,
                        IsRemove = false,
                        ChunkPos = pos,
                        ItemId = SubMeshCacheManager.Instance.GetMeshId(item.ItemID),
                        Pos = new float3(key.X, key.Y, key.Z),
                        WorldId = chunk.WorldId
                    }));
                }

                var cubes = CollectionHelper.CreateNativeArray<ItemGenerateJob.ItemInfoForJob>(items.Count, Allocator.TempJob);
                for (var i = 0; i < items.Count; i++) {
                    cubes[i] = items[i];
                }

                var job = new ItemGenerateJob {
                    Prototype = prototype,
                    Ecb = ecb.AsParallelWriter(),
                    Data = cubes
                };
                var task = job.Schedule(cubes.Length, 256);
                task.Complete();
                cubes.Dispose();
            } else {
                // 本地有物品
                var items = new List<ItemGenerateJob.ItemInfoForJob>();
                foreach (var (key, value) in serverItems) {
                    items.AddRange(value.Select(item => new { item, shouldCreate = !localItems.ContainsKey(key) || !localItems[key].Contains(item) })
                    .Where(@t => !@t.shouldCreate)
                    .Select(@t => new ItemGenerateJob.ItemInfoForJob {
                        IsCreate = true,
                        IsRemove = false,
                        ChunkPos = pos,
                        ItemId = SubMeshCacheManager.Instance.GetMeshId(@t.item.ItemID),
                        Pos = new float3(key.X, key.Y, key.Z),
                        WorldId = chunk.WorldId
                    }));
                }
                foreach (var (key, value) in localItems) {
                    items.AddRange(value.Select(item => new { item, shouldRemove = !serverItems.ContainsKey(key) || !serverItems[key].Contains(item) })
                    .Where(@t => !@t.shouldRemove)
                    .Select(@t => new ItemGenerateJob.ItemInfoForJob {
                        IsCreate = false,
                        IsRemove = true,
                        ChunkPos = pos,
                        ItemId = SubMeshCacheManager.Instance.GetMeshId(@t.item.ItemID),
                        Pos = new float3(key.X, key.Y, key.Z),
                        WorldId = chunk.WorldId
                    }));
                }

                var cubes = CollectionHelper.CreateNativeArray<ItemGenerateJob.ItemInfoForJob>(items.Count, Allocator.TempJob);
                for (var i = 0; i < items.Count; i++) {
                    cubes[i] = items[i];
                }

                var job = new ItemGenerateJob {
                    Prototype = prototype,
                    Ecb = ecb.AsParallelWriter(),
                    Data = cubes
                };
                var task = job.Schedule(cubes.Length, 256);
                task.Complete();
                cubes.Dispose();
            }
            LocalChunkManager.Instance.AddItem(pos, serverItems);
        }
    }
}