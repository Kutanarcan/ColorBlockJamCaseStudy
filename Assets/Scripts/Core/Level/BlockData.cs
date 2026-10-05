using System;

namespace Game.Core
{
    public sealed class BlockData
    {
        public int ColorId;
        public Cell[] Cells = Array.Empty<Cell>();
    }
}
