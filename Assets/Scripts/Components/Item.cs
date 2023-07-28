using System;
using Unity.Entities;

namespace Components {
    public struct Item: ISharedComponentData, IEquatable<Item> {
        public string ItemId;
        
        public bool Equals(Item other) {
            return ItemId == other.ItemId;
        }

        public override int GetHashCode() {
            return ItemId.GetHashCode();
        }
    }
}