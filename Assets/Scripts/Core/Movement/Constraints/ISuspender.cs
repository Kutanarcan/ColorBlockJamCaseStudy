namespace Game.Core
{
    public interface ISuspender : IModifier
    {
        Capability Suspends { get; }
    }
}
