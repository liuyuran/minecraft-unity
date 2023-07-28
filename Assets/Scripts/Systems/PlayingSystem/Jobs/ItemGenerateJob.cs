using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Systems.PlayingSystem.Jobs {
    public struct ItemGenerateJob : IJobParallelFor {
        public struct ItemInfoForJob {
            public bool IsCreate;
            public bool IsRemove;
            public int WorldId;
            public int ItemId;
            public float3 Pos;
            public Vector3 ChunkPos;
            public Entity Entity;
        }

        public Entity Prototype;
        public EntityCommandBuffer.ParallelWriter Ecb;
        // 貌似这里只能用只读数组
        [ReadOnly] public NativeArray<ItemInfoForJob> Data;

        public void Execute(int index) {
            //
        }
    }
}