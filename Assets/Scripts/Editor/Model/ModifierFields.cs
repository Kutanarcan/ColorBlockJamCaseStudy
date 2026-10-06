using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// A modifier's fields as an editable list, read through the DTO's own Write and applied back through its Read.
    /// The editor draws any modifier this way: no per-modifier drawer and no reflection.
    /// </summary>
    public sealed class ModifierFields : IModifierWriter, IModifierReader
    {
        private readonly List<string> keys = new List<string>();
        private readonly List<FieldKind> kinds = new List<FieldKind>();
        private readonly List<int> ints = new List<int>();
        private readonly List<Direction> directions = new List<Direction>();

        public int Count => keys.Count;

        public static ModifierFields Of(ModifierData data)
        {
            var fields = new ModifierFields();
            data.Write(fields);

            return fields;
        }

        public string KeyAt(int index) => keys[index];
        public FieldKind KindAt(int index) => kinds[index];
        public int IntAt(int index) => ints[index];
        public Direction DirectionAt(int index) => directions[index];

        public void SetInt(int index, int value) => ints[index] = value;
        public void SetDirection(int index, Direction value) => directions[index] = value;

        public void ApplyTo(ModifierData data) => data.Read(this);

        void IModifierWriter.Int(string key, int value) => Add(key, FieldKind.Int, value, default);

        void IModifierWriter.Direction(string key, Direction value) => Add(key, FieldKind.Direction, 0, value);

        int IModifierReader.Int(string key) => ints[IndexOf(key, FieldKind.Int)];

        Direction IModifierReader.Direction(string key) => directions[IndexOf(key, FieldKind.Direction)];

        private void Add(string key, FieldKind kind, int intValue, Direction directionValue)
        {
            keys.Add(key);
            kinds.Add(kind);
            ints.Add(intValue);
            directions.Add(directionValue);
        }

        private int IndexOf(string key, FieldKind kind)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] == key && kinds[i] == kind)
                    return i;
            }

            throw new InvalidOperationException($"The modifier reads {kind} '{key}' but never wrote it.");
        }
    }
}
