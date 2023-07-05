using System.Collections.Generic;
using Base.Const;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Chunk = Base.Utils.Chunk;
using Entity = Unity.Entities.Entity;

namespace Systems.Jobs {
    public struct BlockUpdateJob : IJobParallelFor {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public Chunk Chunk;
        [ReadOnly] public Dictionary<Vector3, Entity> BlockEntities;
        [ReadOnly] public NativeArray<Entity> EntityData;

        public void Execute(int index) {
            var currentX = index / ParamConst.ChunkSize / ParamConst.ChunkSize % ParamConst.ChunkSize;
            var currentY = index / ParamConst.ChunkSize % ParamConst.ChunkSize;
            var currentZ = index % ParamConst.ChunkSize;
            var currentPos = new Vector3(currentX, currentY, currentZ);
            Debug.Log(currentPos);
        }
    }
}