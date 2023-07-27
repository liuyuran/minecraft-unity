using Unity.Entities;
using UnityEngine;

namespace Components {
    public struct Item: ISharedComponentData {
        public string ItemId;
    }
}