using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;

namespace Systems {
    [RequireMatchingQueriesForUpdate]
    public partial class SceneControlSystem : SystemBase {
        private EntityQuery _newRequests;

        protected override void OnCreate() {
            _newRequests = GetEntityQuery(typeof(SceneLoader));
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