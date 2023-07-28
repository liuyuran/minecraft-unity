using Components;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;

namespace Systems.PlayingSystem {
    /// <summary>
    /// 场景切换系统，但目前并不知道这个机制有什么用
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class SceneControlSystem : SystemBase {
        private EntityQuery _newRequests;

        protected override void OnCreate() {
            Enabled = false;
            _newRequests = GetEntityQuery(typeof(SceneLoader));
        }

        protected override void OnDestroy() {
            _newRequests.Dispose();
        }

        protected override void OnUpdate() {
            var requests = _newRequests.ToComponentDataArray<SceneLoader>(Allocator.Temp);
            for (var i = 0; i < requests.Length; i += 1) {
                SceneSystem.LoadSceneAsync(World.Unmanaged, requests[i].SceneReference);
            }
            requests.Dispose();
            EntityManager.DestroyEntity(_newRequests);
        }
    }
}