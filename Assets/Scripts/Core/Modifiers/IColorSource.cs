namespace Game.Core
{
    /// <summary>Overrides its entity's color while it is on it. The most recently added source wins.</summary>
    public interface IColorSource : IModifier
    {
        int ColorId { get; }
    }
}
