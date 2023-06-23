using Unity.Entities;

namespace Components {
    public struct EntityGenerator: IComponentData {
        public Entity Player;
        public Entity Cube;
    }
}