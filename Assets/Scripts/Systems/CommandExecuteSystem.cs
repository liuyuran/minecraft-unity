using System.Collections.Generic;
using Managers;
using Systems.Jobs;
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

namespace Systems {
    public partial struct CommandExecuteSystem : ISystem {
        public void OnCreate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var prototype = GetBlockPrototype(entityManager);
            var cubes = CollectionHelper.CreateNativeArray<float3>(1, Allocator.TempJob);
            cubes[0] = new float3(0, 0, 0);

            var job = new BlockGenerateJob {
                Prototype = prototype,
                Ecb = ecb.AsParallelWriter(),
                Pos = cubes
            };
            var task = job.Schedule(cubes.Length, 128);
            task.Complete();
            ecb.Playback(entityManager);
            ecb.Dispose();
            entityManager.DestroyEntity(prototype);
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
                }
            };
            mesh.triangles = new[] {
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
            };
            return mesh;
        }

        private Entity GetBlockPrototype(EntityManager entityManager) {
            var cube = entityManager.CreateEntity();
            var mesh = GenerateBlockMesh();
            const string blockId = "classic:air";
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