namespace Exceptions {
    public class TextureLoadFailedException: System.Exception {
        public TextureLoadFailedException(string blockId, string texture): base($"[blockID: {blockId}]的贴图[{texture}]预加载失败") {}
    }
}