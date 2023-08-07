using Const;
using Managers;
using Unity.Entities;

namespace Systems.SystemGroups {
    public abstract partial class BaseSystemGroup : ComponentSystemGroup {

        protected override void OnUpdate() {
            if (State != GameManager.Instance.State) return;
            base.OnUpdate();
        }

        protected abstract GameState State { get; }
    }
}