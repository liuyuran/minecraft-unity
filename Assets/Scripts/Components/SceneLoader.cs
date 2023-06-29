using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;

namespace Components {
    public struct SceneLoader : IComponentData {
        public EntitySceneReference SceneReference;
    }

#if UNITY_EDITOR
    public class SceneLoaderAuthoring : MonoBehaviour {
        public UnityEditor.SceneAsset scene;

        public class Baker : Baker<SceneLoaderAuthoring> {
            public override void Bake(SceneLoaderAuthoring authoring) {
                var reference = new EntitySceneReference(authoring.scene);
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SceneLoader {
                    SceneReference = reference
                });
            }
        }
    }
#endif
}