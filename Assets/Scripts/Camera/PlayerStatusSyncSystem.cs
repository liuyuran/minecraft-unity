using System;
using Base.Components;
using Base.Manager;
using Unity.Entities;
using UnityEngine;

namespace Camera {
    public partial struct PlayerStatusSyncSystem: ISystem {
        public void OnCreate(ref SystemState state) {
            //
        }
        
        public void OnDestroy(ref SystemState state) {
            //
        }

        public void OnUpdate(ref SystemState state) {
            foreach (var (player, _) in SystemAPI.Query<RefRO<Player>, RefRO<Self>>()) {
                // 也许之后会因为bug出现多个【自身】概念，这里强制锁定为第一个，这样或许可以增加发现问题的概率
                Debug.Log(player.ValueRO.Pos);
                CommandTransferManager.NetworkAdapter?.UpdatePlayerInfo(new Position {
                    X = (long)Math.Round(player.ValueRO.Pos.x),
                    Y = (long)Math.Round(player.ValueRO.Pos.y),
                    Z = (long)Math.Round(player.ValueRO.Pos.z)
                }, new Position());
                break;
            }
        }
    }
}