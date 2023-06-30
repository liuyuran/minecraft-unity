using Components;
using Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Chunk = Base.Utils.Chunk;
using Entity = Unity.Entities.Entity;

namespace Systems.Jobs {
    public struct BlockUpdateJob : IJobParallelFor {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public Chunk Chunk;
        [ReadOnly] public NativeArray<Entity> EntityData;

        public void Execute(int index) {
        }
    }
}