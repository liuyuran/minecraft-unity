using Unity.Entities;
using UnityEngine;

namespace Camera {
    public struct Player : IComponentData {
        public Vector3 Pos;
    }

    public class PlayerAuthoring : UnityEngine.MonoBehaviour {
        public class PlayerBaker : Baker<PlayerAuthoring> {
            public override void Bake(PlayerAuthoring authoring) {
                AddComponent(GetEntity(TransformUsageFlags.Dynamic), new Player {
                    Pos = new Vector3(0, 0, 0)
                });
            }
        }
    }
}