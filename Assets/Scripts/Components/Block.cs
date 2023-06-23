using Unity.Entities;
using UnityEngine;

namespace Components {
    public struct Block: IComponentData {
        public Vector3 Chunk;
    }
}