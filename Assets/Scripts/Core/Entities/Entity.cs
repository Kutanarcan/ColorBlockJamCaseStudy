namespace Game.Core
{
    /// <summary>
    /// Anything that occupies cells. Shape is immutable and relative to <see cref="Position"/>;
    /// a move changes only the position. Every entity can carry modifiers.
    /// </summary>
    public abstract class Entity
    {
        private readonly Cell[] shape;
        private readonly IModifier[] modifiers;

        public int Id { get; }
        public Cell Position { get; internal set; }
        public int CellCount => shape.Length;
        public int ModifierCount { get; private set; }

        /// <param name="modifiers">Owned by the entity from here on; removal compacts it in place.</param>
        protected Entity(int id, Cell position, Cell[] shape, IModifier[] modifiers)
        {
            Id = id;
            Position = position;
            this.shape = shape;
            this.modifiers = modifiers;
            ModifierCount = modifiers.Length;
        }

        /// <summary>The absolute grid cell of shape cell <paramref name="index"/>.</summary>
        public Cell GetCell(int index) => Position + shape[index];

        public IModifier GetModifier(int index) => modifiers[index];

        /// <summary>Removes one modifier and keeps the order of the rest. No allocation.</summary>
        internal void RemoveModifierAt(int index)
        {
            ModifierCount--;

            for (int i = index; i < ModifierCount; i++)
                modifiers[i] = modifiers[i + 1];

            modifiers[ModifierCount] = null;
        }
    }
}
