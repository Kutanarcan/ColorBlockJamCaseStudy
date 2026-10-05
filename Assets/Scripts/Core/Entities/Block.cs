namespace Game.Core
{
    public sealed class Block : Entity
    {
        public int BaseColorId { get; }

        public bool IsExited { get; internal set; }

        public Block(int id, Cell position, Cell[] shape, int baseColorId, IModifier[] modifiers)
            : base(id, position, shape, modifiers)
        {
            BaseColorId = baseColorId;
        }
    }
}
