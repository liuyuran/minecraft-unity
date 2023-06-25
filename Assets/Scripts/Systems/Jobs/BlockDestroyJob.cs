using Components;
using Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Systems.Jobs {
    public struct BlockDestroyJob : IJobParallelFor {
        public EntityCommandBuffer.ParallelWriter Ecb;

        // 貌似这里只能用只读数组
        [ReadOnly] public NativeArray<Entity> Data;

        public void Execute(int index) {
            Ecb.DestroyEntity(index, Data[index]);
        }
    }
}