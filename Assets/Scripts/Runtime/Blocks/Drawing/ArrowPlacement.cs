namespace Game.Runtime
{
    /// <summary>Where a block's arrow goes and how long it is: Arrow_1, Arrow_2 or Arrow_3 (Size).</summary>
    public readonly struct ArrowPlacement
    {
        public Placement Placement { get; }
        public int Size { get; }

        public ArrowPlacement(Placement placement, int size)
        {
            Placement = placement;
            Size = size;
        }

        public override string ToString() => $"{Placement} size {Size}";
    }
}
