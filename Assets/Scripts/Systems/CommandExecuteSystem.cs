using Components;
using Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;

namespace Systems {
    public partial struct CommandExecuteSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BlockGenerator>();
        }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            var entityManager = state.EntityManager;
            if (!SystemAPI.HasSingleton<BlockGenerator>()) {
                return;
            }
            var generator = SystemAPI.GetSingleton<BlockGenerator>();
            var cubes = CollectionHelper.CreateNativeArray<Entity>(1, Allocator.Temp);
            entityManager.Instantiate(generator.ProtoType, cubes);
            foreach (var cube in cubes) {
                var position = new float3(2, 2, 2);
                var transform = SystemAPI.GetComponentRW<LocalTransform>(cube);
                transform.ValueRW.Position = position;
                entityManager.AddComponentData(cube, new LocalToWorld {
                    Value = float4x4.Translate(position)
                });

                var mesh = new Mesh();
                mesh.vertices = new Vector3[] {
                    new Vector3(0, 0, 0),
                    new Vector3(0, 1, 0),
                    new Vector3(1, 1, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(0, 0, 1),
                    new Vector3(0, 1, 1),
                    new Vector3(1, 1, 1),
                    new Vector3(1, 0, 1)
                };
                mesh.triangles = new int[] {
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
                };
                var uvs = new Vector2[mesh.vertices.Length];
                for (var i = 0; i < uvs.Length; i++) {
                    uvs[i] = new Vector2(mesh.vertices[i].x,
                        mesh.vertices[i].y);
                }
                mesh.SetUVs(0,uvs);
                mesh.RecalculateNormals();
                var desc = new RenderMeshDescription(
                    shadowCastingMode: ShadowCastingMode.Off,
                    receiveShadows: false);
                
                var byteArray = File.ReadAllBytes($"{Application.dataPath}/Texture/texture.jpg");
                var texture = new Texture2D(32,32);
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
            }

            state.Enabled = false;
        }
    }
}