using Unity.Collections;
using Unity.Jobs;

namespace Systems.Jobs {
    [GenerateTestsForBurstCompatibility]
    public struct BlockGenerateJob : IJobParallelFor
    {
        public void Execute(int index) {
            throw new System.NotImplementedException();
        }
    }
}