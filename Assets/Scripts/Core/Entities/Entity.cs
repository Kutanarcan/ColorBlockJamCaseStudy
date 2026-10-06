using System.Collections.Generic;

namespace Game.Core
{
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

        public Cell GetCell(int index) => Position + shape[index];

        public IModifier GetModifier(int index) => modifiers[index];
        internal void AddModifier(IModifier modifier) => modifiers.Add(modifier);
        internal bool RemoveModifier(IModifier modifier) => modifiers.Remove(modifier);
    }
}
