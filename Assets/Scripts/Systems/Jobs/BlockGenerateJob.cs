﻿using Components;
using Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Systems.Jobs {
    public struct BlockGenerateJob : IJobParallelFor {
        public struct BlockInfoForJob {
            public int BlockId;
            public float3 Pos;
            public int RenderFlags;
            public Vector3 ChunkPos;
        }

        public Entity Prototype;

        public EntityCommandBuffer.ParallelWriter Ecb;

        // 貌似这里只能用只读数组
        [ReadOnly] public NativeArray<BlockInfoForJob> Data;

        public void Execute(int index) {
            var e = Ecb.Instantiate(index, Prototype);
            // 添加这个组件才能让方块显示在世界里
            Ecb.SetComponent(index, e, new LocalToWorld {
                Value = float4x4.Translate(Data[index].Pos)
            });
            Ecb.SetComponent(index, e, new LocalTransform {
                Position = Data[index].Pos,
                Rotation = quaternion.identity,
                Scale = 1
            });
            Ecb.SetComponent(index, e, new Block());
            Ecb.SetComponent(index, e, SubMeshCacheManager.Instance.GetCubeMesh(
                Data[index].BlockId,
                Data[index].RenderFlags
            ));
            Ecb.SetSharedComponent(index, e, new Chunk {
                Pos = Data[index].ChunkPos
            });
        }
    }
}