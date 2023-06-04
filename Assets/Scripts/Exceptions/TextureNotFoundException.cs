namespace Exceptions {
    public class TextureNotFoundException: System.Exception {
        public TextureNotFoundException(string blockId): base($"未找到[blockID: {blockId}]的贴图") {}
    }
}