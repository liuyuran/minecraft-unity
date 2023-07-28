using Const;

namespace Systems.SystemGroups {
    public partial class MainMenuSystemGroup: BaseSystemGroup {
        protected override GameState State => GameState.Menu;
    }
}