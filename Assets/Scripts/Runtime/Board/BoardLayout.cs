using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The static shape of a level, built once from <see cref="LevelData"/> (D87): which cells are solid
    /// (walls and doors) and which door sits where. Blocks are not part of it; their cells count as open.
    /// Board space: cell (x, y) spans [2x, 2x + 2] on X and [2y, 2y + 2] on Z, origin at cell (0, 0).
    /// </summary>
    public sealed class BoardLayout
    {
        public const float CellSize = 2f;

        private readonly bool[] solid;
        private readonly DoorData[] doors;

        public int Width { get; }
        public int Height { get; }

        public BoardLayout(LevelData level)
        {
            Width = level.Width;
            Height = level.Height;
            solid = new bool[Width * Height];
            doors = new DoorData[Width * Height];

            for (int i = 0; i < level.Walls.Length; i++)
                MarkSolid(level.Walls[i].Cells);

            for (int i = 0; i < level.Doors.Length; i++)
                MarkDoor(level.Doors[i]);
        }

        /// <summary>Outside the grid counts as solid, so the frame's outer side draws nothing.</summary>
        public bool IsSolid(Cell cell) => !IsInside(cell) || solid[IndexOf(cell)];

        /// <summary>The door covering this cell, or null.</summary>
        public DoorData DoorAt(Cell cell) => IsInside(cell) ? doors[IndexOf(cell)] : null;

        /// <summary>The whole grid as a box in board space, <paramref name="height"/> tall: what the camera keeps in view.</summary>
        public Bounds Extent(float height)
        {
            var size = new Vector3(Width * CellSize, height, Height * CellSize);

            return new Bounds(size * 0.5f, size);
        }

        public static Vector3 CellCenter(Cell cell) =>
            new Vector3((cell.X + 0.5f) * CellSize, 0f, (cell.Y + 0.5f) * CellSize);

        private bool IsInside(Cell cell) => cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;

        private int IndexOf(Cell cell) => cell.Y * Width + cell.X;

        private void MarkSolid(Cell[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
                solid[IndexOf(cells[i])] = true;
        }

        private void MarkDoor(DoorData door)
        {
            MarkSolid(door.Cells);

            for (int i = 0; i < door.Cells.Length; i++)
                doors[IndexOf(door.Cells[i])] = door;
        }
    }
}
