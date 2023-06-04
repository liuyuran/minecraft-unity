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
    
    [Serializable]
    [MaterialProperty("_Texture")]
    public struct BlockTexture: ISharedComponentData, IEquatable<BlockTexture> {
        public Texture2D texture;

        public bool Equals(BlockTexture other) {
            return Equals(texture, other.texture);
        }

        public override bool Equals(object obj) {
            return obj is BlockTexture other && Equals(other);
        }

        public override int GetHashCode() {
            return (texture != null ? texture.GetHashCode() : 0);
        }
    }
}