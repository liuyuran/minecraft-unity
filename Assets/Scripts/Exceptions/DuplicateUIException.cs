namespace Exceptions {
    public class DuplicateUIException: System.Exception {
        public DuplicateUIException(string ui): base($"UI[id: {ui}]注册失败，UI名称不能重复") {}
    }
}