namespace Game.Core
{
    public sealed class Block : Entity
    {
        public int ColorId { get; }

        public Block(int id, Cell position, Cell[] shape, int colorId)
            : base(id, position, shape)
        {
            ColorId = colorId;
        }
    }
}
