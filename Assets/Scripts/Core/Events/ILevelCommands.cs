namespace Game.Core
{
    public interface ILevelCommands
    {
        void AddModifier(Entity entity, IModifier modifier);
        void RemoveModifier(Entity entity, IModifier modifier);

        void MoveEntity(Entity entity, Cell offset);

        void Fail();
        void AddTime(float seconds);
    }
}
