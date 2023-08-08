using Components;
using Managers;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Entity = Unity.Entities.Entity;

namespace Systems.PlayingSystem.Jobs {
    public struct BlockUpdateJob : IJobParallelFor {
        public struct BlockInfoForJob {
            public bool ShouldCreate;
            public Entity? Entity;
            public int WorldId;
            public int BlockId;
            public float3 Pos;
            public int RenderFlags;
            public Vector3 ChunkPos;
        }

        public EntityCommandBuffer.ParallelWriter Ecb;
        public Entity Prototype;
        [ReadOnly] public NativeArray<BlockInfoForJob> Data;

        public void Execute(int index) {
            var item = Data[index];
            if (item is { ShouldCreate: false, Entity: not null }) {
                Ecb.DestroyEntity(index, item.Entity.Value);
            }
            var e = Ecb.Instantiate(index, Prototype);
            // 添加这个组件才能让方块显示在世界里
            Ecb.SetComponent(index, e, new LocalToWorld {
                Value = float4x4.Translate(item.Pos)
            });
            Ecb.SetComponent(index, e, new LocalTransform {
                Position = item.Pos,
                Rotation = quaternion.identity,
                Scale = 1
            });
            Ecb.SetComponent(index, e, new Block {
                Pos = item.Pos
            });
            Ecb.SetSharedComponent(index, e, new Components.Chunk {
                Pos = item.ChunkPos
            });
            Ecb.SetSharedComponent(index, e, new BlockTransform {
                ChunkPos = Data[index].ChunkPos,
                BlockPos = Data[index].Pos
            });
            Ecb.SetSharedComponent(index, e, new GameWorld {
                WorldId = item.WorldId
            });
            Ecb.SetComponent(index, e, SubMeshCacheManager.Instance.GetCubeMesh(
                item.BlockId,
                item.RenderFlags
            ));
            // Ecb.SetComponent(index, e, SubMeshCacheManager.Instance.Collider);
        }
    }
}