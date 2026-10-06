using System;
using System.Collections.Generic;
using Game.Core;
using Game.LevelEditor;

namespace Game.Tests.LevelEditor
{
    /// <summary>
    /// Test-only: builds a <see cref="LevelModel"/> from an ASCII map. The first row is the top (highest y).
    /// '#' = wall (one entity), '.' = empty; every other symbol is declared with Door or Block and is one entity.
    /// </summary>
    internal sealed class AsciiModel
    {
        private const char WallSymbol = '#';
        private const char EmptySymbol = '.';

        private readonly string[] rows;
        private readonly List<char> order = new List<char>();
        private readonly Dictionary<char, Func<LevelModel, int>> creators = new Dictionary<char, Func<LevelModel, int>>();

        private AsciiModel(string[] rows) => this.rows = rows;

        public static AsciiModel Parse(params string[] rows) => new AsciiModel(rows);

        public AsciiModel Door(char symbol, int colorId, Direction direction) =>
            Declare(symbol, model => model.AddDoor(colorId, direction));

        public AsciiModel Block(char symbol, int colorId) => Declare(symbol, model => model.AddBlock(colorId));

        public LevelModel Build()
        {
            var model = new LevelModel(rows[0].Length, rows.Length);
            var ids = new Dictionary<char, int> { { WallSymbol, model.AddWall() } };

            foreach (char symbol in order)
            {
                ids.Add(symbol, creators[symbol](model));
            }

            for (int row = 0; row < rows.Length; row++)
            {
                int y = rows.Length - 1 - row;

                for (int x = 0; x < rows[row].Length; x++)
                {
                    char symbol = rows[row][x];

                    if (symbol == EmptySymbol)
                        continue;

                    if (!ids.TryGetValue(symbol, out int id))
                        throw new ArgumentException($"Symbol '{symbol}' at ({x}, {y}) is not declared.");

                    model.Paint(new Cell(x, y), id);
                }
            }

            return model;
        }

        private AsciiModel Declare(char symbol, Func<LevelModel, int> create)
        {
            order.Add(symbol);
            creators.Add(symbol, create);

            return this;
        }
    }
}
