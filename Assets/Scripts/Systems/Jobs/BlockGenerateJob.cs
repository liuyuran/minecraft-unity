using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Jobs {
    [BurstCompile]
    public struct BlockGenerateJob : IJobParallelFor {
        public Entity Prototype;
        public EntityCommandBuffer.ParallelWriter Ecb;
        // 貌似这里只能用只读数组
        [ReadOnly] public NativeArray<float3> Pos;

        public void Execute(int index) {
            var e = Ecb.Instantiate(index, Prototype);
            // 添加这个组件才能让方块显示在世界里
            Ecb.SetComponent(index, e, new LocalToWorld {
                Value = float4x4.Translate(Pos[index])
            });
        }
    }
}