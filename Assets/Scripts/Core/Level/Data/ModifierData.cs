namespace Game.Core
{
    public abstract class ModifierData
    {
        /// <summary>The name this modifier is saved under. Unique within a <see cref="ModifierCatalog"/>.</summary>
        public abstract string TypeName { get; }

        public abstract IModifier ToModifier();

        // Each DTO saves and loads its own fields, so no serializer needs reflection (IL2CPP safe).
        public abstract void Write(IModifierWriter writer);
        public abstract void Read(IModifierReader reader);

        public virtual Result Validate() => Result.Success();
    }
}
