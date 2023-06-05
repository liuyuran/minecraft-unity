using Components.Tags;
using Managers;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Systems {
    public partial class ChangeTextureSystem : SystemBase {
        private static readonly int Texture2D1 = Shader.PropertyToID("_Texture2D");

        protected override void OnUpdate() {
            Entities.WithAll<NeedSetTexture>()
                .ForEach((ref RenderMesh renderMesh) => {
                    var material = renderMesh.material;
                    material.SetTexture(Texture2D1, BlockTypeManager.Instance.GetMergedTexture());
                }).ScheduleParallel();
        }
    }
}