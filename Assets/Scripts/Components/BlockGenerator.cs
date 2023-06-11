using Unity.Entities;
using UnityEngine;

namespace Monos {
    public struct BlockGenerator: IComponentData {
        public Entity ProtoType;
    }
}