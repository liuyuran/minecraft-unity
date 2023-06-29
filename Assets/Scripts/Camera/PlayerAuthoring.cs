using Unity.Entities;
using UnityEngine;

namespace Camera {
    public struct Player : IComponentData {
        public Vector3 Pos;
        public Vector3 Forward;
    }

    public struct Self : IComponentData { }

    public class PlayerAuthoring : UnityEngine.MonoBehaviour {
        public class PlayerBaker : Baker<PlayerAuthoring> {
            public override void Bake(PlayerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Player {
                    Pos = new Vector3(0, 0, 0),
                    Forward = new Vector3(0, 0, 0)
                });
                AddComponent(entity, new Self());
            }
        }
    }
}