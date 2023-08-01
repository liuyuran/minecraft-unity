namespace Exceptions {
    public class UINotFoundException: System.Exception {
        public UINotFoundException(string ui): base($"UI[id: {ui}]未注册") {}
    }
}