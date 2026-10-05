namespace Game.Core
{
    /// <summary>Passive. Read by <see cref="Rope"/> when the carrying block exits.</summary>
    public sealed class Scissors : IModifier
    {
        public int ColorId { get; }

        public Scissors(int colorId) => ColorId = colorId;
    }
}
