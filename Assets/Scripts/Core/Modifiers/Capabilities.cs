namespace Game.Core
{
    /// <summary>
    /// The central rules: an entity keeps a capability unless a modifier on it suspends it,
    /// a block moves only in directions every constraint allows, and a door accepts a block on a cell
    /// only if every accept rule agrees. Modifiers declare; this decides.
    /// </summary>
    public static class Capabilities
    {
        public static bool CanMove(Entity entity, Direction direction) =>
            !IsSuspended(entity, Capability.Move) && ConstraintsAllow(entity, direction);

        public static bool CanExit(Entity entity) => !IsSuspended(entity, Capability.Exit);

        public static bool CanAccept(Door door, Cell cell, Block block) =>
            !IsSuspended(door, Capability.Accept) && AcceptRulesAllow(door, cell, block);

        private static bool IsSuspended(Entity entity, Capability capability)
        {
            for (int i = 0; i < entity.ModifierCount; i++)
            {
                if (entity.GetModifier(i) is ISuspender suspender && (suspender.Suspends & capability) != 0)
                    return true;
            }

            return false;
        }

        private static bool ConstraintsAllow(Entity entity, Direction direction)
        {
            for (int i = 0; i < entity.ModifierCount; i++)
            {
                if (entity.GetModifier(i) is IMoveConstraint constraint && !constraint.Allows(direction))
                    return false;
            }

            return true;
        }

        private static bool AcceptRulesAllow(Door door, Cell cell, Block block)
        {
            for (int i = 0; i < door.ModifierCount; i++)
            {
                if (door.GetModifier(i) is IExitAcceptRule rule && !rule.Accepts(door, cell, block))
                    return false;
            }

            return true;
        }
    }
}
