using Unity.Entities;
using UnityEngine;

namespace Components {
    public struct BlockTransform: ISharedComponentData {
        public Vector3 ChunkPos;
        public Vector3 BlockPos;
    }
}