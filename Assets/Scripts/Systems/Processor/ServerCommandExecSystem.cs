﻿using System.Threading;
using Base;
using Base.Manager;
using Base.Messages;
using Components;
using Managers;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems.Processor {
    /// <summary>
    /// 从基础库的支持类中获取命令执行队列，然后执行
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct ServerCommandExecSystem : ISystem {
        private EntityQuery _query;
        
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EntityGenerator>();
            _query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Chunk>()
                .Build(state.EntityManager);
            new Thread(() => { Game.Start(""); }).Start();
            Thread.Sleep(1000);
            CommandTransferManager.NetworkAdapter?.SendToServer(new PlayerJoinEvent {
                Nickname = "Kamoeth"
            });
        }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            SubMeshCacheManager.Instance.GetMeshId("classic:dirt"); // 这里只是为了触发Instance初始化逻辑
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            // 开始刷新区块
            var prototype = GetBlockPrototype(entityManager);
            while (CommandTransferManager.NetworkAdapter?.TryGetFromServer(out var message) ?? false) {
                if (message == null) return;
                switch (message) {
                    case ChunkUpdateEvent chunkUpdateEvent:
                        GenerateChunkBlocks(entityManager, ecb, prototype, chunkUpdateEvent);
                        break;
                }
            }
            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}