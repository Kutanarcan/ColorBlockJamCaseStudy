using System;

namespace Game.Core
{
    public sealed class LevelData
    {
        public int SchemaVersion;
        public int Width;
        public int Height;
        public float TimeLimit;

        public WallData[] Walls = Array.Empty<WallData>();
        public DoorData[] Doors = Array.Empty<DoorData>();
        public BlockData[] Blocks = Array.Empty<BlockData>();
    }
}
