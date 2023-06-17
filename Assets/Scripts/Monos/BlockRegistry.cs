using Components;
using Unity.Entities;
using UnityEngine;

namespace Monos {
    public class BlockRegistry: MonoBehaviour {
        public GameObject cube;
        
        public class BlockBaker: Baker<BlockRegistry> {
            public override void Bake(BlockRegistry authoring) {
                AddComponent(GetEntity(TransformUsageFlags.None), new BlockGenerator {
                    cube = GetEntity(authoring.cube, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}