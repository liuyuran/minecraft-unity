using Const;

namespace Managers {
    public class GameManager {
        public static GameManager Instance { get; } = new();
        public GameState State { get; private set; } = GameState.Menu;

        public void SetState(GameState state) {
            State = state;
        }
    }
}