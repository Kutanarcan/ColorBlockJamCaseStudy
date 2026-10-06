namespace Game.LevelEditor
{
    /// <summary>A broken rule. <see cref="CellIndex"/> points the window at the cell, or is None for the whole level.</summary>
    public readonly struct RuleViolation
    {
        public string Message { get; }
        public int CellIndex { get; }

        public RuleViolation(string message, int cellIndex = LevelModel.None)
        {
            Message = message;
            CellIndex = cellIndex;
        }

        public override string ToString() => Message;
    }
}
