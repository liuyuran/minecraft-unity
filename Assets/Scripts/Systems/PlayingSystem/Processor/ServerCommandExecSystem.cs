using System.Threading;
using Base;
using Base.Events.ClientEvent;
using Base.Events.ServerEvent;
using Base.Manager;
using Components;
using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Utils;

namespace Systems.PlayingSystem.Processor {
    /// <summary>
    /// 从基础库的支持类中获取命令执行队列，然后执行
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct ServerCommandExecSystem : ISystem {
        private EntityQuery _blockQuery;
        private EntityQuery _itemQuery;
        private static bool _isInit;
        
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EntityGenerator>();
            _blockQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Chunk, Components.Block>()
                .Build(state.EntityManager);
            _itemQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Chunk, Item>()
                .Build(state.EntityManager);
            UnitySystemConsoleRedirect.Redirect();
            new Thread(() => { Game.Start(""); }).Start();
            Thread.Sleep(1000);
            CommandTransferManager.NetworkAdapter?.SendToServer(new PlayerJoinEvent {
                Nickname = "Kamoeth"
            });
        }

        public void OnDestroy(ref SystemState state) {
            _blockQuery.Dispose();
            _itemQuery.Dispose();
        }

        public void OnUpdate(ref SystemState state) {
            SubMeshCacheManager.Instance.GetMeshId("classic:dirt"); // 这里只是为了触发Instance初始化逻辑
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            // 开始刷新区块
            if (_isInit) {
                GetBlockPrototype(entityManager);
                GetItemPrototype(entityManager);
                _isInit = true;
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
        }
    }
}