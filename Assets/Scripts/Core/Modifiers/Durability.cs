namespace Game.Core
{
    /// <summary>Only a number. It locks nothing by itself; what it means is up to the modifier carrying it.</summary>
    public sealed class Durability
    {
        public int Remaining { get; private set; }
        public bool IsDepleted => Remaining <= 0;

        public Durability(int count) => Remaining = count;

        internal void Reduce(int amount)
        {
            Remaining -= amount;

            if (Remaining < 0)
                Remaining = 0;
        }
    }
}
