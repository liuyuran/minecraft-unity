using System.Collections.Generic;
using System.Threading;
using Base;
using Base.Const;
using Base.Manager;
using Base.Messages;
using Components;
using Managers;
using Systems.Jobs;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems.Processor {
    /// <summary>
    /// 从基础库的支持类中获取命令执行队列，然后执行
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct ServerCommandExecSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EntityGenerator>();
            SubMeshCacheManager.Instance.GetMeshId("classic:air"); // 这里只是为了触发Instance初始化逻辑
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
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            // 开始刷新区块
            var prototype = GetBlockPrototype(entityManager);
            while (CommandTransferManager.NetworkAdapter?.TryGetFromClient(out var message) ?? false) {
                if (message == null) return;
                switch (message) {
                    case ChunkUpdateEvent chunkUpdateEvent:
                        GenerateChunkBlocks(ecb, prototype, chunkUpdateEvent);
                        break;
                }
            }
            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}