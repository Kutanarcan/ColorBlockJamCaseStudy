using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Anything that occupies cells. Shape is immutable and relative to <see cref="Position"/>;
    /// a move changes only the position. Every entity can carry modifiers.
    /// </summary>
    public abstract class Entity
    {
        private readonly Cell[] shape;
        private readonly List<IModifier> modifiers;

        public int Id { get; }
        public Cell Position { get; internal set; }
        public int CellCount => shape.Length;
        public int ModifierCount => modifiers.Count;

        protected Entity(int id, Cell position, Cell[] shape, IModifier[] modifiers)
        {
            Id = id;
            Position = position;
            this.shape = shape;
            this.modifiers = new List<IModifier>(modifiers);
        }

        /// <summary>The absolute grid cell of shape cell <paramref name="index"/>.</summary>
        public Cell GetCell(int index) => Position + shape[index];

        public IModifier GetModifier(int index) => modifiers[index];

        internal void AddModifier(IModifier modifier) => modifiers.Add(modifier);

        /// <summary>Keeps the order of the rest. Removing a modifier that is already gone does nothing.</summary>
        internal void RemoveModifier(IModifier modifier) => modifiers.Remove(modifier);
    }
}
