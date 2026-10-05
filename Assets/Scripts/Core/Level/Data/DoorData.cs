using System;

namespace Game.Core
{
    public sealed class DoorData
    {
        public int ColorId;
        public Direction Direction;
        public Cell[] Cells = Array.Empty<Cell>();
    }
}
