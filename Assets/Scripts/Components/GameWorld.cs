using Unity.Entities;

namespace Components {
    public struct GameWorld: ISharedComponentData {
        public int WorldId;
    }
}