using System.Collections.Generic;
using System.Threading;
using Base;
using Base.Const;
using Base.Manager;
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
            Debug.Log($"ChunkQueue: {chunkQueue.Length}");
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var prototype = GetBlockPrototype(entityManager);
            var transformArray = new List<float3>();
            foreach (var chunk in chunkQueue) {
                for (var x = 0; x < ParamConst.ChunkSize; x++) {
                    for (var y = 0; y < ParamConst.ChunkSize; y++) {
                        for (var z = 0; z < ParamConst.ChunkSize; z++) {
                            var block = chunk.GetBlock(x, y, z);
                            if (block.IsAir) continue;
                            transformArray.Add(new float3(
                                x + chunk.Position.X * ParamConst.ChunkSize, 
                                y + chunk.Position.Y * ParamConst.ChunkSize, 
                                z + chunk.Position.Z * ParamConst.ChunkSize));
                        }
                    }
                }
            }
            var cubes = CollectionHelper.CreateNativeArray<float3>(transformArray.Count, Allocator.TempJob);
            for (var i = 0; i < transformArray.Count; i++) {
                cubes[i] = transformArray[i];
            }
            var job = new BlockGenerateJob {
                Prototype = prototype,
                Ecb = ecb.AsParallelWriter(),
                Pos = cubes
            };
            var task = job.Schedule(cubes.Length, 128);
            task.Complete();
            ecb.Playback(entityManager);
            ecb.Dispose();
            state.Enabled = false;
        }

        private Mesh GenerateBlockMesh() {
            var mesh = new Mesh {
                vertices = new Vector3[] {
                    // front face
                    new(0, 0, 0),
                    new(0, 1, 0),
                    new(1, 1, 0),
                    new(1, 0, 0),
                    // back face
                    new(1, 0, 1),
                    new(1, 1, 1),
                    new(0, 1, 1),
                    new(0, 0, 1),
                    // top face
                    new(0, 1, 0),
                    new(0, 1, 1),
                    new(1, 1, 1),
                    new(1, 1, 0),
                    // bottom face
                    new(0, 0, 1),
                    new(0, 0, 0),
                    new(1, 0, 0),
                    new(1, 0, 1),
                    // left face
                    new(0, 0, 1),
                    new(0, 1, 1),
                    new(0, 1, 0),
                    new(0, 0, 0),
                    // right face
                    new(1, 0, 0),
                    new(1, 1, 0),
                    new(1, 1, 1),
                    new(1, 0, 1)
                },
                triangles = new[] {
                    // front face
                    0, 1, 2,
                    2, 3, 0,
                    // back face
                    4, 5, 6,
                    6, 7, 4,
                    // top face
                    8, 9, 10,
                    10, 11, 8,
                    // bottom face
                    12, 13, 14,
                    14, 15, 12,
                    // left face
                    16, 17, 18,
                    18, 19, 16,
                    // right face
                    20, 21, 22,
                    22, 23, 20
                }
            };
            return mesh;
        }

        private Entity GetBlockPrototype(EntityManager entityManager) {
            var generator = SystemAPI.GetSingleton<BlockGenerator>();
            var cube = generator.cube;
            var mesh = GenerateBlockMesh();
            const string blockId = "classic:air";
            // 注意，这里的材质是URP的材质，不是Unity的材质，且GetMergedTexture必须在GetBlockTexture之前调用
            // 不然就会报材质找不到的错误，原因很简单，调用材质合成之后，子材质映射才会生效
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) {
                mainTexture = BlockTypeManager.Instance.GetMergedTexture()
            };
            var uvs = new List<Vector2>();
            uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.south));
            uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.north));
            uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.up));
            uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.down));
            uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.west));
            uvs.AddRange(BlockTypeManager.Instance.GetBlockTexture(blockId, Direction.east));

            mesh.SetUVs(0, uvs.ToArray());
            mesh.RecalculateNormals();
            var desc = new RenderMeshDescription(
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: false);
            var renderMeshArray = new RenderMeshArray(
                new[] { material },
                new[] { mesh }
            );
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