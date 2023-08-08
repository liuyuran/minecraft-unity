using System.Collections.Generic;
using System.Linq;
using Const;
using Managers;
using Unity.Entities;

namespace Systems.SystemGroups {
    public abstract partial class BaseSystemGroup : ComponentSystemGroup {

        protected override void OnUpdate() {
            if (!State.Contains(GameManager.Instance.State)) return;
            base.OnUpdate();
        }

        protected abstract IEnumerable<GameState> State { get; }
    }
}