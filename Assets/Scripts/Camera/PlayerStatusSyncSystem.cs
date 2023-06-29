using Base.Manager;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Transform = Base.Components.Transform;
using Vector3 = System.Numerics.Vector3;

namespace Camera {
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct PlayerStatusSyncSystem : ISystem {
        private EntityQuery _query;
        
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<Player>();
            state.RequireForUpdate<Self>();
            var entityManager = state.EntityManager;
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Player, Self>()
                .Build(entityManager);
        }

        public void OnDestroy(ref SystemState state) {
            _query.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            var player = _query.GetSingleton<Player>();
            CommandTransferManager.NetworkAdapter?.UpdatePlayerInfo(new Transform {
                Position = new Vector3(
                    player.Pos.x,
                    player.Pos.y,
                    player.Pos.z
                ),
                Forward = new Vector3(
                    player.Forward.x,
                    player.Forward.y,
                    player.Forward.z
                )
            });
        }
    }
}