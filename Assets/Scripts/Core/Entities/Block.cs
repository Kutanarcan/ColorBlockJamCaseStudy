namespace Game.Core
{
    public sealed class Block : Entity
    {
        public int ColorId { get; }
        public bool IsExited { get; internal set; }

        public Block(int id, Cell position, Cell[] shape, int colorId, IModifier[] modifiers)
            : base(id, position, shape, modifiers)
        {
            ColorId = colorId;
        }
    }
}
