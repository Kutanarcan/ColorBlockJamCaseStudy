namespace Game.Core
{
    /// <summary>
    /// Anything that occupies cells. Shape is immutable and relative to <see cref="Position"/>;
    /// a move changes only the position.
    /// </summary>
    public abstract class Entity
    {
        private readonly Cell[] shape;

        public int Id { get; }
        public Cell Position { get; internal set; }
        public int CellCount => shape.Length;

        protected Entity(int id, Cell position, Cell[] shape)
        {
            Id = id;
            Position = position;
            this.shape = shape;
        }

        /// <summary>The absolute grid cell of shape cell <paramref name="index"/>.</summary>
        public Cell GetCell(int index) => Position + shape[index];
    }
}
