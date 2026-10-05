namespace Game.Core
{
    /// <summary>Freezes its block: no move, no exit, until <c>count</c> exits of any block have happened.</summary>
    public sealed class Ice : ISuspender, IDurable
    {
        public Capability Suspends => Capability.Move | Capability.Exit;
        public Durability Durability { get; }

        public Ice(int count) => Durability = new Durability(count);

        public int AmountFor(Block exited) => 1;
    }
}
