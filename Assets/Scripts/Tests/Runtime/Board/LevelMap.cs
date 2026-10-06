using System.Collections.Generic;
using Game.Core;

namespace Game.Tests.Runtime
{
    /// <summary>
    /// Test-only: a <see cref="LevelData"/> from an ASCII map, first row on top. '#' = wall, '.' = open,
    /// a digit = a door of that colorId facing <c>doorDirection</c>. Every door cell is its own door entity,
    /// so runs across entities are exercised.
    /// </summary>
    internal static class LevelMap
    {
        public static LevelData Parse(Direction doorDirection, params string[] rows)
        {
            var walls = new List<Cell>();
            var doors = new List<DoorData>();

            for (int row = 0; row < rows.Length; row++)
            {
                int y = rows.Length - 1 - row;

                for (int x = 0; x < rows[row].Length; x++)
                {
                    char symbol = rows[row][x];

                    if (symbol == '#')
                        walls.Add(new Cell(x, y));
                    else if (char.IsDigit(symbol))
                        doors.Add(new DoorData
                        {
                            ColorId = symbol - '0', Direction = doorDirection, Cells = new[] { new Cell(x, y) }
                        });
                }
            }

            return new LevelData
            {
                SchemaVersion = LevelValidator.SupportedSchemaVersion,
                Width = rows[0].Length,
                Height = rows.Length,
                Walls = new[] { new WallData { Cells = walls.ToArray() } },
                Doors = doors.ToArray()
            };
        }
    }
}
