namespace Game.Runtime
{
    /// <summary>One door piece: centered on its run of door cells, as long as the run, in the run's color (D98).</summary>
    public readonly struct DoorRun
    {
        public Placement Placement { get; }
        public int Length { get; }
        public int ColorId { get; }

        public DoorRun(Placement placement, int length, int colorId)
        {
            Placement = placement;
            Length = length;
            ColorId = colorId;
        }

        public override string ToString() => $"{Placement} length {Length} color {ColorId}";
    }
}
