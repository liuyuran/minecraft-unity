using System.Collections.Generic;
using System.Threading;
using Base;
using Base.Const;
using Base.Manager;
using Camera;
using Components;
using Managers;
using Systems.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Entity = Unity.Entities.Entity;
using EntityManager = Unity.Entities.EntityManager;

namespace Systems {
    /// <summary>
    /// 清理区块
    /// </summary>
    [BurstCompile]
    public partial struct BlockCleanSystem : ISystem {
        public void OnCreate(ref SystemState state) {
            //
        }

        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            // 下一行看似没有意义，但是必须在这里预先获取Instance对象，不然会出问题
            SubMeshCacheManager.Instance.GetMeshId("classic:air");
            var entityManager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            // 开始卸载区块
            foreach (var (player, _) in SystemAPI.Query<RefRO<Player>, RefRO<Self>>()) {
                // 也许之后会因为bug出现多个【自身】概念，这里强制锁定为第一个，这样或许可以增加发现问题的概率
                LocalChunkManager.Instance.AutoUnloadChunk(ecb, player.ValueRO.Pos);    
                break;
            }
            ecb.Playback(entityManager);
            ecb.Dispose();
        }
    }
}