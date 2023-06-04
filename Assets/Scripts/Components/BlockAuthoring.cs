using Unity.Entities;
using UnityEngine;

namespace Components {
    public class BlockAuthoring : MonoBehaviour {
        public GameObject cubePrefab;

        public class Baker : Baker<BlockAuthoring> {
            public override void Bake(BlockAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BlockId {
                    blockId = 0
                });
                AddComponent(entity, new BlockGenerator {
                    ProtoType = GetEntity(authoring.cubePrefab, TransformUsageFlags.None)
                });
            }
        }
    }
}