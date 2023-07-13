using Unity.Entities;
using UnityEngine;

namespace Components {
    public struct Block: IComponentData {
        public int BlockId;
        public Vector3 Pos;
    }
}