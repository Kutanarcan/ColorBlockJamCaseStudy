using System;

namespace Game.Core
{
    /// <summary>Immutable level definition. Cells are absolute grid coordinates.</summary>
    public sealed class LevelData
    {
        public int Width;
        public int Height;
        public WallData[] Walls = Array.Empty<WallData>();
        public DoorData[] Doors = Array.Empty<DoorData>();
        public BlockData[] Blocks = Array.Empty<BlockData>();
    }
}
