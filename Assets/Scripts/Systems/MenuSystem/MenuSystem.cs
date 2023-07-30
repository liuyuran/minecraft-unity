using Systems.SystemGroups;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace Systems.MenuSystem {
    [BurstCompile]
    [UpdateInGroup(typeof(MainMenuSystemGroup))]
    public partial struct MenuSystem : ISystem {
        private Entity _canvasEntity;
        
        public void OnCreate(ref SystemState state) {
            var entityManager = state.EntityManager;
            _canvasEntity = entityManager.CreateEntity(
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
        }

        public void OnDestroy(ref SystemState state) {
            var entityManager = state.EntityManager;
            entityManager.DestroyEntity(_canvasEntity);
        }

        public void OnUpdate(ref SystemState state) {
            //
        }
    }
}