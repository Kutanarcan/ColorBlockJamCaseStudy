namespace Game.Core
{
    /// <summary>Format-neutral field output for a modifier DTO. A new field kind adds a method here.</summary>
    public interface IModifierWriter
    {
        void Int(string key, int value);
        void Direction(string key, Direction value);
    }
}
