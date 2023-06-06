using Components.Tags;
using Managers;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Systems {
    public partial class ChangeTextureSystem : SystemBase {
        private static readonly int Texture2D1 = Shader.PropertyToID("_Texture2D");

        protected override void OnUpdate() {
            // Entities
            //     .WithoutBurst()
            //     .WithStructuralChanges()
            //     .WithAll<NeedSetTexture>()
            //     .ForEach((Entity entity, in RenderMesh renderMesh) => {
            //         Debug.Log("Set Texture");
            //         var material = renderMesh.material;
            //         material.mainTexture = BlockTypeManager.Instance.GetMergedTexture();
            //         EntityManager.RemoveComponent<NeedSetTexture>(entity);
            //         EntityManager.SetSharedComponentManaged(entity, new RenderMesh {
            //             mesh = renderMesh.mesh,
            //             material = material
            //         });
            //         Debug.Log("Set Texture Done");
            //     }).Run();
            // Enabled = false;
        }
    }
}