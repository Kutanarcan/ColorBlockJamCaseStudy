using System;

namespace Game.Core
{
    public sealed class Door : Entity
    {
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
