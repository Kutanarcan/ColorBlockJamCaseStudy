using System;

namespace Game.Core
{
    /// <summary>Accepts blocks of its color moving in <see cref="Direction"/>; a wall from any other side.</summary>
    public sealed class Door : Entity
    {
        public int ColorId { get; }
        public Direction Direction { get; }

        public Door(int id, Cell position, Cell[] shape, int colorId, Direction direction)
            : base(id, position, shape, Array.Empty<IModifier>())
        {
            ColorId = colorId;
            Direction = direction;
        }
    }
}
