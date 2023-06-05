using Components;
using Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems {
    public partial struct CommandExecuteSystem: ISystem {

        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BlockGenerator>();
        }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            var entityManager = state.EntityManager;
            var generator = SystemAPI.GetSingleton<BlockGenerator>();
            var cubes = CollectionHelper.CreateNativeArray<Entity>(1, Allocator.Temp);
            entityManager.Instantiate(generator.ProtoType, cubes);
            foreach (var cube in cubes) {
                var position = new float3(0, 0, 0);
                var transform = SystemAPI.GetComponentRW<LocalTransform>(cube);
                transform.ValueRW.Position = position;
            }

            state.Enabled = false;
        }
    }
}