using Base.Items;

namespace Systems.PlayingSystem.Processor {
    public partial struct ServerCommandExecSystem {
        private bool IsEqual(Item clientBlock, Item serverBlock) {
            return serverBlock.ID == clientBlock.ID;
        }
    }
}