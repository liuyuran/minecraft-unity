using Unity.Entities;
using UnityEngine;

namespace Components {
    public struct Chunk: ISharedComponentData {
        public Vector3 Pos;
    }
}