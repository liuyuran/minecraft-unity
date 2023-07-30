using Const;
using Utils;

namespace Managers {
    public class GameManager {
        public static GameManager Instance { get; } = new();
        public GameState State { get; private set; } = GameState.Menu;

        private GameManager() {
            UnitySystemConsoleRedirect.Redirect();
        }

        public void SetState(GameState state) {
            State = state;
        }
    }
}