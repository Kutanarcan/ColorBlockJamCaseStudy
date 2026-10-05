namespace Game.Core
{
    public enum LevelErrorKind
    {
        UnsupportedSchemaVersion,
        InvalidGridSize,
        EmptyEntity,
        OutOfBounds,
        Overlap,
        InvalidModifier
    }
}
