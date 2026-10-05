namespace Game.Core
{
    public sealed class Grid
    {
        public const int Empty = -1;

        private readonly int[] cells;

        public int Width { get; }
        public int Height { get; }

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new int[width * height];
            for (int i = 0; i < cells.Length; i++)
                cells[i] = Empty;
        }

        public int IndexOf(Cell cell) => cell.Y * Width + cell.X;
        public Cell CellOf(int index) => new Cell(index % Width, index / Width);

        public int Get(Cell cell) => cells[IndexOf(cell)];

        internal void Set(Cell cell, int entityId) => cells[IndexOf(cell)] = entityId;
    }
}
