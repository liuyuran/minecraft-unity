using System.Collections.Generic;
using Const;

namespace Systems.SystemGroups {
    public partial class MainMenuSystemGroup: BaseSystemGroup {
        protected override IEnumerable<GameState> State => new []{GameState.Menu};
    }
}