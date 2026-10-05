using System;
using Game.Core;
using Newtonsoft.Json.Linq;

namespace Game.LevelIO
{
    /// <summary>LevelData to JObject. Only modifiers in the catalog are written, so every saved level can be loaded.</summary>
    internal sealed class LevelJsonWriter
    {
        private readonly ModifierCatalog catalog;

        public LevelJsonWriter(ModifierCatalog catalog) => this.catalog = catalog;

        public JObject Write(LevelData level)
        {
            return new JObject
            {
                { "schemaVersion", level.SchemaVersion },
                { "width", level.Width },
                { "height", level.Height },
                { "timeLimit", level.TimeLimit },
                { "walls", WriteList(level.Walls, WriteWall) },
                { "doors", WriteList(level.Doors, WriteDoor) },
                { "blocks", WriteList(level.Blocks, WriteBlock) }
            };
        }

        private static JObject WriteWall(WallData wall) =>
            new JObject { { "cells", WriteCells(wall.Cells) } };

        private static JObject WriteDoor(DoorData door) =>
            new JObject
            {
                { "colorId", door.ColorId },
                { "direction", DirectionNames.ToName(door.Direction) },
                { "cells", WriteCells(door.Cells) }
            };

        private JObject WriteBlock(BlockData block) =>
            new JObject
            {
                { "colorId", block.ColorId },
                { "cells", WriteCells(block.Cells) },
                { "modifiers", WriteList(block.Modifiers, WriteModifier) }
            };

        private JObject WriteModifier(ModifierData data)
        {
            if (!catalog.Contains(data.TypeName))
                throw new InvalidOperationException(
                    $"Modifier type '{data.TypeName}' is not in the catalog, so the level could not be loaded back.");

            var json = new JObject { { "type", data.TypeName } };
            data.Write(new JsonModifierWriter(json));

            return json;
        }

        private static JArray WriteCells(Cell[] cells)
        {
            var array = new JArray();

            foreach (Cell cell in cells)
            {
                array.Add(new JArray(cell.X, cell.Y));
            }

            return array;
        }

        private static JArray WriteList<T>(T[] items, Func<T, JObject> write)
        {
            var array = new JArray();

            foreach (T item in items)
            {
                array.Add(write(item));
            }

            return array;
        }
    }
}
