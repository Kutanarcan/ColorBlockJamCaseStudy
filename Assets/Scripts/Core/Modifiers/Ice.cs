namespace Game.Core
{
    /// <summary>Holds the block in place until <c>count</c> exits of any block have happened.</summary>
    public sealed class Ice : ISuspender, IDurable
    {
        public Capability Suspends => Capability.Move | Capability.Exit;
        public Durability Durability { get; }

        public Ice(int count) => Durability = new Durability(count);

        public int AmountFor(Block exited) => 1;
    }
}
