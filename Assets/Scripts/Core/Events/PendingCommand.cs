namespace Game.Core
{
    /// <summary>
    /// One recorded command. A struct so the buffer holds commands inline: recording never allocates
    /// while the buffer has capacity. Only the fields its <see cref="Kind"/> needs are set.
    /// </summary>
    internal readonly struct PendingCommand
    {
        public readonly CommandKind Kind;
        public readonly Entity Entity;
        public readonly IModifier Modifier;
        public readonly Cell Offset;
        public readonly float Seconds;

        public PendingCommand(CommandKind kind, Entity entity = null, IModifier modifier = null,
            Cell offset = default, float seconds = 0f)
        {
            Kind = kind;
            Entity = entity;
            Modifier = modifier;
            Offset = offset;
            Seconds = seconds;
        }
    }
}
