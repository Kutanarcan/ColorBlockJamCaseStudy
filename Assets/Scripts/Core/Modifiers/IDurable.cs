namespace Game.Core
{
    /// <summary>
    /// Carries a count that exits wear down. The modifier only says how much one exit counts;
    /// <see cref="ExitResolver"/> reduces it and removes the modifier when it is depleted.
    /// </summary>
    public interface IDurable : IModifier
    {
        Durability Durability { get; }

        int AmountFor(Block exited);
    }
}
