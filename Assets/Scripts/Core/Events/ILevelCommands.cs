namespace Game.Core
{
    /// <summary>
    /// The only way a listener changes the level. Commands are recorded during an event pass and applied
    /// in order when it ends, so no listener sees a half-applied effect. New effects combine these primitives.
    /// </summary>
    public interface ILevelCommands
    {
        void AddModifier(Entity entity, IModifier modifier);
        void RemoveModifier(Entity entity, IModifier modifier);

        /// <summary>Applied only if every target cell is empty or the entity's own; otherwise skipped.</summary>
        void MoveEntity(Entity entity, Cell offset);

        void Fail();
        void AddTime(float seconds);
    }
}
