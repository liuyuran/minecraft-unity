namespace Exceptions {
    public class BlockRegistryForbiddenException: System.Exception {
        public BlockRegistryForbiddenException(string msg): base($"方块注册失败，原因为: {msg}") {}
    }
}