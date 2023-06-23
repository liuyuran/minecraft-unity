using Components;
using Unity.Entities;
using UnityEngine;

namespace Monos {
    public class EntityRegistry: MonoBehaviour {
        public GameObject player;
        public GameObject cube;
        
        public class EntityBaker: Baker<EntityRegistry> {
            public override void Bake(EntityRegistry authoring) {
                AddComponent(GetEntity(TransformUsageFlags.None), new EntityGenerator {
                    Player = GetEntity(authoring.player, TransformUsageFlags.Dynamic),
                    Cube = GetEntity(authoring.cube, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}