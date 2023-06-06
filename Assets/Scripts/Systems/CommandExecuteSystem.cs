using Components;
using Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

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
                var position = new float3(0, 0, 0);
                var transform = SystemAPI.GetComponentRW<LocalTransform>(cube);
                transform.ValueRW.Position = position;

                var mesh = new Mesh();
                var desc = new RenderMeshDescription(
                    shadowCastingMode: ShadowCastingMode.Off,
                    receiveShadows: false);
                
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) {
                    mainTexture = BlockTypeManager.Instance.GetMergedTexture()
                };

                // Create an array of mesh and material required for runtime rendering.
                var renderMeshArray = new RenderMeshArray(
                    new[] { material },
                    new[] { mesh }
                );
                // Create empty base entity
                var prototype = entityManager.CreateEntity();
                // Call AddComponents to populate base entity with the components required
                // by Entities Graphics
                RenderMeshUtility.AddComponents(
                    prototype,
                    entityManager,
                    desc,
                    renderMeshArray,
                    MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0));
            }

            state.Enabled = false;
        }
    }
}