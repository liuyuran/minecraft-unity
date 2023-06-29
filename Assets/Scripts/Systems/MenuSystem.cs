using Systems.SystemGroups;
using Unity.Burst;
using Unity.Entities;

namespace Systems {
    [BurstCompile]
    [UpdateInGroup(typeof(MainMenuSystemGroup))]
    public partial struct MenuSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            //
        }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            //
        }
    }
}