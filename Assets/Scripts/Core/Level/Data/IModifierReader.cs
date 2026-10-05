namespace Game.Core
{
    /// <summary>Format-neutral field input for a modifier DTO. A missing or wrongly typed field fails the load.</summary>
    public interface IModifierReader
    {
        int Int(string key);
        Direction Direction(string key);
    }
}
