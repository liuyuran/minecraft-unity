using Systems.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;

namespace Systems {
    public partial struct CommandExecuteSystem : ISystem {
        public void OnCreate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            var cube = entityManager.CreateEntity();
            var position = new float3(2, 2, 2);
            var mesh = new Mesh {
                vertices = new Vector3[] {
                    new(0, 0, 0),
                    new(0, 1, 0),
                    new(1, 1, 0),
                    new(1, 0, 0),
                    new(0, 0, 1),
                    new(0, 1, 1),
                    new(1, 1, 1),
                    new(1, 0, 1)
                },
                triangles = new int[] {
                    // front face
                    0, 1, 2,
                    2, 3, 0,
                    // top face
                    1, 5, 6,
                    6, 2, 1,
                    // back face
                    7, 6, 5,
                    5, 4, 7,
                    // bottom face
                    4, 0, 3,
                    3, 7, 4,
                    // left face
                    4, 5, 1,
                    1, 0, 4,
                    // right face
                    3, 2, 6,
                    6, 7, 3
                }
            };
            var uvs = new Vector2[mesh.vertices.Length];
            for (var i = 0; i < uvs.Length; i++) {
                uvs[i] = new Vector2(mesh.vertices[i].x,
                    mesh.vertices[i].y);
            }

            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            var desc = new RenderMeshDescription(
                shadowCastingMode: ShadowCastingMode.Off,
                receiveShadows: false);

            var byteArray = File.ReadAllBytes($"{Application.dataPath}/Texture/texture.jpg");
            var texture = new Texture2D(32, 32);
            var isLoaded = texture.LoadImage(byteArray);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) {
                mainTexture = texture
            };
            // Create an array of mesh and material required for runtime rendering.
            var renderMeshArray = new RenderMeshArray(
                new[] { material },
                new[] { mesh }
            );
            // Call AddComponents to populate base entity with the components required
            // by Entities Graphics
            RenderMeshUtility.AddComponents(
                cube,
                entityManager,
                desc,
                renderMeshArray,
                MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));

            var job = new BlockGenerateJob {
                Prototype = cube,
                Ecb = ecb.AsParallelWriter(),
                Pos = position
            };
            var task = job.Schedule(1, 128);
            task.Complete();

            ecb.Playback(entityManager);
            ecb.Dispose();
            entityManager.DestroyEntity(cube);

            state.Enabled = false;
        }
    }
}