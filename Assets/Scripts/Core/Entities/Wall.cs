namespace Game.Core
{
    public sealed class Wall : Entity
    {
        public Wall(int id, Cell position, Cell[] shape)
            : base(id, position, shape)
        {
        }
    }
}
