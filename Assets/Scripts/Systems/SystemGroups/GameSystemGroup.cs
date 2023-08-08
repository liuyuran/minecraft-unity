using System.Collections.Generic;
using Const;

namespace Systems.SystemGroups {
    public partial class GameSystemGroup: BaseSystemGroup {
        protected override IEnumerable<GameState> State => new []{GameState.Loading, GameState.Playing};
    }
}