namespace Game.Core
{
    public sealed class Durability
    {
        public int Remaining { get; private set; }
        public bool IsDepleted => Remaining <= 0;

        public Durability(int count) => Remaining = count;

        public void WearDown(Entity owner, IModifier carrier, int amount, ILevelCommands commands)
        {
            Remaining -= amount;

            if (Remaining < 0)
                Remaining = 0;

            if (IsDepleted)
                commands.RemoveModifier(owner, carrier);
        }
    }
}
