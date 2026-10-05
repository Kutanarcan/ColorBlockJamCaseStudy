using System;

namespace Game.Core
{
    /// <summary>Accepts blocks of its color moving in <see cref="Direction"/>; a wall from any other side.</summary>
    public sealed class Door : Entity
    {
        /// <summary>The color it was built with. Read the effective color through <see cref="Colors.Of"/>.</summary>
        public int BaseColorId { get; }

        public Direction Direction { get; }

        public Door(int id, Cell position, Cell[] shape, int baseColorId, Direction direction)
            : base(id, position, shape, Array.Empty<IModifier>())
        {
            BaseColorId = baseColorId;
            Direction = direction;
        }
    }
}
