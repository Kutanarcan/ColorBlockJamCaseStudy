namespace Game.Runtime
{
    /// <summary>One mesh of a block: which part, and where it goes in board space.</summary>
    public readonly struct BlockPart
    {
        public BlockPartKind Kind { get; }
        public Placement Placement { get; }

        public BlockPart(BlockPartKind kind, Placement placement)
        {
            Kind = kind;
            Placement = placement;
        }

        public override string ToString() => $"{Kind} {Placement}";
    }
}
