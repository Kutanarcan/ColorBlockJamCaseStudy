namespace Game.Core
{
    /// <summary>While this modifier is on a block, the block loses <see cref="Suspends"/>.</summary>
    public interface ISuspender : IModifier
    {
        Capability Suspends { get; }
    }
}
