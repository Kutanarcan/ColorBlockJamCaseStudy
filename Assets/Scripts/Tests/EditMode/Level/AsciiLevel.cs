using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.Tests.EditMode
{
    /// <summary>
    /// Test-only: builds a <see cref="LevelData"/> from an ASCII map. Never a level format.
    /// The first row is the top of the grid (highest y).
    /// '#' = wall (all walls form one entity), '.' = empty, any other symbol must be declared
    /// with <see cref="Block"/> or <see cref="Door"/>; each symbol is one entity.
    /// </summary>
    internal sealed class AsciiLevel
    {
        private const char WallSymbol = '#';
        private const char EmptySymbol = '.';

        private readonly string[] rows;
        private readonly List<char> blockSymbols = new List<char>();
        private readonly List<char> doorSymbols = new List<char>();
        private readonly Dictionary<char, int> colors = new Dictionary<char, int>();
        private readonly Dictionary<char, Direction> doorDirections = new Dictionary<char, Direction>();
        private readonly Dictionary<char, ModifierData[]> blockModifiers = new Dictionary<char, ModifierData[]>();
        private float timeLimit;

        private AsciiLevel(string[] rows) => this.rows = rows;

        public AsciiLevel TimeLimit(float seconds)
        {
            timeLimit = seconds;
            return this;
        }

        public static AsciiLevel Parse(params string[] rows) => new AsciiLevel(rows);

        public AsciiLevel Block(char symbol, int colorId, params ModifierData[] modifiers)
        {
            blockSymbols.Add(symbol);
            colors.Add(symbol, colorId);
            blockModifiers.Add(symbol, modifiers);
            return this;
        }

        public AsciiLevel Door(char symbol, int colorId, Direction direction)
        {
            doorSymbols.Add(symbol);
            colors.Add(symbol, colorId);
            doorDirections.Add(symbol, direction);
            return this;
        }

        public LevelData Build()
        {
            Dictionary<char, List<Cell>> cellsBySymbol = CollectCells(out int width);
            var level = new LevelData
            {
                SchemaVersion = LevelValidator.SupportedSchemaVersion,
                Width = width,
                Height = rows.Length,
                TimeLimit = timeLimit
            };

            if (cellsBySymbol.TryGetValue(WallSymbol, out List<Cell> wallCells))
                level.Walls = new[] { new WallData { Cells = wallCells.ToArray() } };

            level.Doors = new DoorData[doorSymbols.Count];
            for (int i = 0; i < doorSymbols.Count; i++)
            {
                char s = doorSymbols[i];
                level.Doors[i] = new DoorData
                {
                    ColorId = colors[s], Direction = doorDirections[s], Cells = CellsOf(cellsBySymbol, s)
                };
            }

            level.Blocks = new BlockData[blockSymbols.Count];
            for (int i = 0; i < blockSymbols.Count; i++)
            {
                char s = blockSymbols[i];
                level.Blocks[i] = new BlockData
                {
                    ColorId = colors[s], Cells = CellsOf(cellsBySymbol, s), Modifiers = blockModifiers[s]
                };
            }

            return level;
        }

        private Dictionary<char, List<Cell>> CollectCells(out int width)
        {
            width = rows[0].Length;
            var cellsBySymbol = new Dictionary<char, List<Cell>>();

            for (int row = 0; row < rows.Length; row++)
            {
                if (rows[row].Length != width)
                    throw new ArgumentException($"Row {row} has length {rows[row].Length}, expected {width}.");

                int y = rows.Length - 1 - row;
                for (int x = 0; x < width; x++)
                {
                    char s = rows[row][x];
                    if (s == EmptySymbol)
                        continue;
                    if (s != WallSymbol && !colors.ContainsKey(s))
                        throw new ArgumentException($"Symbol '{s}' at ({x}, {y}) is not declared.");

                    if (!cellsBySymbol.TryGetValue(s, out List<Cell> cells))
                        cellsBySymbol.Add(s, cells = new List<Cell>());
                    cells.Add(new Cell(x, y));
                }
            }

            return cellsBySymbol;
        }

        private static Cell[] CellsOf(Dictionary<char, List<Cell>> cellsBySymbol, char symbol)
        {
            if (!cellsBySymbol.TryGetValue(symbol, out List<Cell> cells))
                throw new ArgumentException($"Symbol '{symbol}' is declared but not on the map.");
            return cells.ToArray();
        }
    }
}
