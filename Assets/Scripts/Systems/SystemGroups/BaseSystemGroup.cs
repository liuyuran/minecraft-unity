using Const;
using Managers;
using Unity.Entities;

namespace Systems.SystemGroups {
    public abstract partial class BaseSystemGroup : ComponentSystemGroup {

        protected override void OnUpdate() {
            Enabled = State == GameManager.Instance.State;
            if (Enabled) base.OnUpdate();
        }

        protected abstract GameState State { get; }
    }
}