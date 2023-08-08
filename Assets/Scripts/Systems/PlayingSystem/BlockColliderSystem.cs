using Camera;
using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Player = Camera.Player;

namespace Systems.PlayingSystem {
    /// <summary>
    /// 尝试根据距离动态赋予或解除物理特性
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct BlockColliderSystem : ISystem {
        private EntityQuery _transformQuery;
        private EntityQuery _playerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _transformQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<LocalTransform>()
                .Build(state.EntityManager);
            _playerQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Player, Self>()
                .Build(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
            _transformQuery.Dispose();
            _playerQuery.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            var entityManager = state.EntityManager;
            var player = _playerQuery.GetSingletonEntity();
            var playerPos = entityManager.GetComponentData<LocalTransform>(player).Position;
            var entities = _transformQuery.ToEntityArray(Allocator.TempJob);
            var translations = _transformQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var collider = SubMeshCacheManager.Instance.Collider;
            for (var i = 0; i < entities.Length; i++) {
                var position = translations[i].Position;
                var entity = entities[i];
                var distance = Vector3.Distance(playerPos, position);
                var hasCollider = entityManager.HasComponent<PhysicsCollider>(entity);
                switch (distance) {
                    case <= 50 when hasCollider:
                    case > 50 when !hasCollider:
                        continue;
                    case <= 50:
                        entityManager.AddComponentData(entity, collider);
                        break;
                    default:
                        entityManager.RemoveComponent<PhysicsCollider>(entity);
                        break;
                }
            }

            entities.Dispose();
            translations.Dispose();
        }
    }
}