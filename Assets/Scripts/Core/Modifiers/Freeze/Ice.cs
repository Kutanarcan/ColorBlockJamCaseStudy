namespace Game.Core
{
    public sealed class Ice : ISuspender, IExitListener
    {
        public Capability Suspends => Capability.Move | Capability.Exit;
        public Durability Durability { get; }

        public Ice(int count) => Durability = new Durability(count);

        public void OnExited(Entity owner, Block exited, ILevelCommands commands) =>
            Durability.WearDown(owner, this, 1, commands);
    }
}
