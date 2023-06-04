namespace Exceptions {
    public class DuplicateBlockIdException: System.Exception {
        public DuplicateBlockIdException(string blockId): base($"发现了重复的blockID: {blockId}") {}
    }
}