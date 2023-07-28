using Base.Events.ClientEvent;
using Base.Manager;
using Camera;
using Components;
using Systems.SystemGroups;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Systems.PlayingSystem {
    /// <summary>
    /// 拾取系统，一定距离内物品会飞向角色并消失
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(GameSystemGroup))]
    public partial struct PickupSystem: ISystem {
        private const int PickDistance = 3;
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
            //
        }
        
        public void OnUpdate(ref SystemState state) {
            var player = _query.GetSingleton<Player>();
            var colliderBuffer = new Collider[10];
            int size;
            do {
                size = Physics.OverlapSphereNonAlloc(player.Pos, PickDistance, colliderBuffer);
                for (var index = 0; index < size; index++) {
                    var collider = colliderBuffer[index];
                    var item = collider.GetComponent<Item>();
                    CommandTransferManager.NetworkAdapter?.SendToServer(new PickUpEvent {
                        ItemId = item.ItemId
                    });
                }
            } while (size > 0);
        }
    }
}