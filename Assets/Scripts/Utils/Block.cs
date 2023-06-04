namespace Utils {
    /// <summary>
    /// 地形基础构成，所有方块的基类
    /// </summary>
    public abstract class Block {
        public virtual string ID => "";
        public virtual string Texture => "";
    }
}