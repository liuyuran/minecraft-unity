using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems.Jobs {
    [BurstCompile]
    public struct BlockGenerateJob : IJobParallelFor {
        public Entity Prototype;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float3 Pos;

        public void Execute(int index) {
            var e = Ecb.Instantiate(index, Prototype);
            Ecb.SetComponent(index, e, new LocalToWorld {
                Value = float4x4.Translate(Pos)
            });
        }
    }
}