namespace Game.Core
{
    /// <summary>One rope of one color. Cut when a block carrying matching <see cref="Scissors"/> exits.</summary>
    public sealed class Rope : ISuspender, IDurable
    {
        public int ColorId { get; }
        public Capability Suspends => Capability.Move | Capability.Exit;
        public Durability Durability { get; }

        public Rope(int colorId)
        {
            ColorId = colorId;
            Durability = new Durability(1);
        }

        public int AmountFor(Block exited)
        {
            int scissors = 0;

            for (int i = 0; i < exited.ModifierCount; i++)
            {
                if (exited.GetModifier(i) is Scissors s && s.ColorId == ColorId)
                    scissors++;
            }

            return scissors;
        }
    }
}
