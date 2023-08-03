using System.Runtime.InteropServices;
using Base.Events.ServerEvent;
using Camera;
using Components;
using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Player = Base.Components.Player;

namespace Systems.PlayingSystem.Processor {
    /// <summary>
    /// 从基础库的支持类中获取命令执行队列，然后执行
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    [StructLayout(LayoutKind.Auto)]
    public partial struct ServerCommandExecSystem : ISystem {
        private EntityQuery _blockQuery;
        private EntityQuery _itemQuery;
        private EntityQuery _playerQuery;
        private static bool _isInit;
        
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EntityGenerator>();
            _blockQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Chunk, Block>()
                .Build(state.EntityManager);
            _itemQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Chunk, Item>()
                .Build(state.EntityManager);
            _playerQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Player, Self>()
                .Build(state.EntityManager);
        }

        public void OnDestroy(ref SystemState state) {
            _blockQuery.Dispose();
            _itemQuery.Dispose();
            _playerQuery.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            SubMeshCacheManager.Instance.GetMeshId("classic:dirt"); // 这里只是为了触发Instance初始化逻辑
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            // 开始刷新区块
            if (!_isInit) {
                GetBlockPrototype(entityManager);
                GetItemPrototype(entityManager);
            }
            var generator = SystemAPI.GetSingleton<EntityGenerator>();
            while (Base.Manager.CommandTransferManager.NetworkAdapter?.TryGetFromServer(out var message) ?? false) {
                if (message == null) return;
                switch (message) {
                    case ChunkUpdateEvent chunkUpdateEvent:
                        GenerateChunkBlocks(generator, ecb, chunkUpdateEvent);
                        break;
                }
            }
            
            ecb.Playback(entityManager);
            ecb.Dispose();
            if (_isInit) return;
            _isInit = true;
            GameManager.Instance.SetState(Const.GameState.Playing);
            // 第一次生成地形后，再给角色赋予重力属性
            var physicsGravityFactor = new Unity.Physics.PhysicsGravityFactor { Value = 1.0f };
            entityManager.SetComponentData(_playerQuery.GetSingletonEntity(), physicsGravityFactor);
        }
    }
}