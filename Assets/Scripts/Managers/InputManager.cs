namespace Managers {
    /// <summary>
    /// 键位配置管理器
    /// </summary>
    public class InputManager {
        public static InputManager Instance { get; } = new();
        
        private KeyActionSettings _keyActionSettings;
        public KeyActionSettings.StandardActions CurrentPlan { get; private set; }

        private InputManager() {
            _keyActionSettings = new KeyActionSettings();
            _keyActionSettings.Enable();
            CurrentPlan = _keyActionSettings.standard;
        }
        
        public void Reset() {
            _keyActionSettings.Dispose();
            _keyActionSettings = new KeyActionSettings();
            _keyActionSettings.Enable();
            CurrentPlan = _keyActionSettings.standard;
        }
    }
}