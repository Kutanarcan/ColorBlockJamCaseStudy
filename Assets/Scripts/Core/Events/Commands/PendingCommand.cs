namespace Game.Core
{
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
