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
        private readonly List<Axis> axes = new List<Axis>();

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
        public Axis AxisAt(int index) => axes[index];

        public void SetInt(int index, int value) => ints[index] = value;
        public void SetDirection(int index, Direction value) => directions[index] = value;
        public void SetAxis(int index, Axis value) => axes[index] = value;

        public void ApplyTo(ModifierData data) => data.Read(this);

        void IModifierWriter.Int(string key, int value)
        {
            Add(key, FieldKind.Int);
            ints[Count - 1] = value;
        }

        void IModifierWriter.Direction(string key, Direction value)
        {
            Add(key, FieldKind.Direction);
            directions[Count - 1] = value;
        }

        void IModifierWriter.Axis(string key, Axis value)
        {
            Add(key, FieldKind.Axis);
            axes[Count - 1] = value;
        }

        int IModifierReader.Int(string key) => ints[IndexOf(key, FieldKind.Int)];

        Direction IModifierReader.Direction(string key) => directions[IndexOf(key, FieldKind.Direction)];

        Axis IModifierReader.Axis(string key) => axes[IndexOf(key, FieldKind.Axis)];

        /// <summary>One row with every kind's default; the caller sets the value of its own kind.</summary>
        private void Add(string key, FieldKind kind)
        {
            keys.Add(key);
            kinds.Add(kind);
            ints.Add(0);
            directions.Add(default);
            axes.Add(default);
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
