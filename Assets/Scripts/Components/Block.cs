using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Components {
    public struct BlockGenerator: IComponentData {
        public Entity ProtoType;
    }
    
    [Serializable]
    [MaterialProperty("_BlockID")]
    public struct BlockId: IComponentData {
        public float blockId;
    }
}