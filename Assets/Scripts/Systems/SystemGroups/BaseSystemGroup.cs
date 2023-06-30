using Unity.Entities;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.SystemGroups {
    public abstract partial class BaseSystemGroup : ComponentSystemGroup {
        private bool _initialized;

        protected override void OnCreate() {
            base.OnCreate();
            _initialized = false;
        }

        protected override void OnUpdate() {
            // if (!_initialized) {
            //     if (SceneManager.GetActiveScene().isLoaded) {
            //         var subScene = Object.FindFirstObjectByType<SubScene>();
            //         if (subScene != null) {
            //             Enabled = AuthoringSceneName == subScene.gameObject.scene.name;
            //         } else {
            //             Enabled = false;
            //         }
            //
            //         _initialized = true;
            //     }
            // }

            base.OnUpdate();
        }

        protected abstract string AuthoringSceneName { get; }
    }
}