using Const;

namespace Systems.SystemGroups {
    public partial class GameSystemGroup: BaseSystemGroup {
        protected override GameState State => GameState.Playing;
    }
}