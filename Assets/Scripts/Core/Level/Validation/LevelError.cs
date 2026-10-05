namespace Game.Core
{
    public readonly struct LevelError
    {
        public LevelErrorKind Kind { get; }
        public string Detail { get; }

        public LevelError(LevelErrorKind kind, string detail)
        {
            Kind = kind;
            Detail = detail;
        }

        public override string ToString() => $"{Kind}: {Detail}";
    }
}
