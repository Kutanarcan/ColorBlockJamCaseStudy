using System;
using Game.Core;
using Newtonsoft.Json.Linq;

namespace Game.LevelIO
{
    /// <summary>JObject to LevelData. Scalar fields are required; entity and modifier lists may be left out.</summary>
    internal sealed class LevelJsonReader
    {
        private const string Root = "level";

        private readonly ModifierCatalog catalog;

        public LevelJsonReader(ModifierCatalog catalog) => this.catalog = catalog;

        public LevelData Read(JObject json)
        {
            return new LevelData
            {
                SchemaVersion = JsonRead.Int(json, "schemaVersion", Root),
                Width = JsonRead.Int(json, "width", Root),
                Height = JsonRead.Int(json, "height", Root),
                TimeLimit = JsonRead.Float(json, "timeLimit", Root),
                Walls = ReadList(json, "walls", Root, ReadWall),
                Doors = ReadList(json, "doors", Root, ReadDoor),
                Blocks = ReadList(json, "blocks", Root, ReadBlock)
            };
        }

        private static WallData ReadWall(JObject json, string path) =>
            new WallData { Cells = ReadCells(json, path) };

        private static DoorData ReadDoor(JObject json, string path) =>
            new DoorData
            {
                ColorId = JsonRead.Int(json, "colorId", path),
                Direction = JsonRead.Direction(json, "direction", path),
                Cells = ReadCells(json, path)
            };

        private BlockData ReadBlock(JObject json, string path) =>
            new BlockData
            {
                ColorId = JsonRead.Int(json, "colorId", path),
                Cells = ReadCells(json, path),
                Modifiers = ReadList(json, "modifiers", path, ReadModifier)
            };

        private ModifierData ReadModifier(JObject json, string path)
        {
            string name = JsonRead.String(json, "type", path);

            if (!catalog.TryCreate(name, out ModifierData data))
                throw new LevelFormatException($"{path} has unknown modifier type '{name}'.");

            data.Read(new JsonModifierReader(json, path));

            return data;
        }

        private static Cell[] ReadCells(JObject json, string path)
        {
            JArray array = JsonRead.Array(json, "cells", path);
            var cells = new Cell[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                cells[i] = JsonRead.Cell(array[i], $"{path}.cells[{i}]");
            }

            return cells;
        }

        private static T[] ReadList<T>(JObject json, string key, string path, Func<JObject, string, T> read)
        {
            JArray array = JsonRead.OptionalArray(json, key, path);

            if (array == null)
                return Array.Empty<T>();

            var items = new T[array.Count];

            for (int i = 0; i < array.Count; i++)
            {
                string itemPath = $"{path}.{key}[{i}]";
                items[i] = read(JsonRead.Object(array[i], itemPath), itemPath);
            }

            return items;
        }
    }
}
