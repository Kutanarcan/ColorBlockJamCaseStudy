namespace Game.Core
{
    public static class Capabilities
    {
        public static bool CanMove(Entity entity, Direction direction)
        {
            return !IsSuspended(entity, Capability.Move) 
                && ConstraintsAllow(entity, direction);
        }

        public static bool CanExit(Entity entity) => !IsSuspended(entity, Capability.Exit);

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
    }
}
