using System.Collections.Generic;
using System.Threading;
using Base;
using Base.Const;
using Base.Manager;
using Base.Utils;
using Components;
using Managers;
using Systems.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using Entity = Unity.Entities.Entity;
using EntityManager = Unity.Entities.EntityManager;
using Material = UnityEngine.Material;

namespace Systems {
    /// <summary>
    /// 从基础库的支持类中获取命令执行队列，然后执行
    /// </summary>
    [BurstCompile]
    public partial struct CommandExecuteSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BlockGenerator>();
            new Thread(() => { Game.Start(""); }).Start();
            Thread.Sleep(1000);
            CommandTransferManager.NetworkAdapter?.JoinGame("Kamoeth");
        }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            var chunkQueue = CommandTransferManager.NetworkAdapter?.GetChunkForUser();
            if (chunkQueue == null || chunkQueue.Length == 0) return;
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var prototype = GetBlockPrototype(entityManager);
            var transformArray = new List<BlockGenerateJob.BlockInfoForJob>();
            foreach (var chunk in chunkQueue) {
                for (var x = 0; x < ParamConst.ChunkSize; x++) {
                    for (var y = 0; y < ParamConst.ChunkSize; y++) {
                        for (var z = 0; z < ParamConst.ChunkSize; z++) {
                            var block = chunk.GetBlock(x, y, z);
                            if (block.Transparent) continue;
                            transformArray.Add(new BlockGenerateJob.BlockInfoForJob {
                                Pos = new float3(
                                    x + chunk.Position.X * ParamConst.ChunkSize,
                                    y + chunk.Position.Y * ParamConst.ChunkSize,
                                    z + chunk.Position.Z * ParamConst.ChunkSize
                                ),
                                RenderFlags = block.RenderFlags
                            });
                        }
                    }
                }
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
            var task = job.Schedule(cubes.Length, 128);
            task.Complete();
            ecb.Playback(entityManager);
            ecb.Dispose();
            state.Enabled = false;
        }

        private Entity GetBlockPrototype(EntityManager entityManager) {
            var generator = SystemAPI.GetSingleton<BlockGenerator>();
            var cube = generator.cube;
            const string blockId = "classic:air";
            var desc = new RenderMeshDescription(
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: false);
            var renderMeshArray = SubMeshCacheManager.Instance.GetCubeMesh(blockId, Chunk.Up | Chunk.Down | Chunk.Back | Chunk.Front | Chunk.Left | Chunk.Right);
            RenderMeshUtility.AddComponents(
                cube,
                entityManager,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
            return cube;
        }
    }
}