using Game.Core;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// The geometry of one block's exit, in board space (FINDINGS exit): where it rests before sliding, which way it
    /// slides, the cut plane inside the door, and its rows counted from the front (row 0 meets the door first).
    /// The front edge is where the block's front row meets the door cell; the cut sits <c>clipOffset</c> beyond it
    /// (0.5 = the middle of the door piece, which covers the approach half of the door cell).
    /// </summary>
    public sealed class ExitPath
    {
        private readonly Cell[] cells;
        private readonly Cell step;
        private readonly int front;
        private readonly float clipOffset;

        public Vector3 Rest { get; }
        public Vector3 Normal { get; }
        public Vector4 ClipPlane { get; }
        public int RowCount { get; }

        /// <summary>How far the block slides from <see cref="Rest"/> until its last row is past the cut.</summary>
        public float TotalDistance => RowCount * BoardLayout.CellSize + clipOffset;

        public int CellCount => cells.Length;

        public ExitPath(Block block, Direction direction, float clipOffset)
        {
            this.clipOffset = clipOffset;
            step = direction.ToOffset();
            cells = new Cell[block.CellCount];

            Normal = new Vector3(step.X, 0f, step.Y);
            Rest = BoardLayout.ToBoard(new Vector2(block.Position.X, block.Position.Y), 0f);

            int back = int.MaxValue;
            front = int.MinValue;
            float frontCenter = float.MinValue;

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = block.GetCell(i);
                int along = Along(cells[i]);
                front = Mathf.Max(front, along);
                back = Mathf.Min(back, along);
                frontCenter = Mathf.Max(frontCenter, Vector3.Dot(BoardLayout.CellCenter(cells[i]), Normal));
            }

            RowCount = front - back + 1;
            float cut = frontCenter + BoardLayout.CellSize * 0.5f + clipOffset;
            ClipPlane = new Vector4(Normal.x, Normal.y, Normal.z, cut);
        }

        public Cell GetCell(int index) => cells[index];

        /// <summary>0 for the front row, counting back.</summary>
        public int RowOf(Cell cell) => front - Along(cell);

        /// <summary>How far the block has slid from <see cref="Rest"/> when the center of a row reaches the cut.</summary>
        public float DistanceToRow(int row) => (row + 0.5f) * BoardLayout.CellSize + clipOffset;

        /// <summary>A cell's center on the cut line, the moment its row reaches it.</summary>
        public Vector3 CenterAtCut(Cell cell) => BoardLayout.CellCenter(cell) + Normal * DistanceToRow(RowOf(cell));

        private int Along(Cell cell) => cell.X * step.X + cell.Y * step.Y;
    }
}
