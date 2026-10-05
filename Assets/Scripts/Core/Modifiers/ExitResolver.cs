namespace Game.Core
{
    /// <summary>
    /// Applies an exit to every entity still on the board, in id order: each <see cref="IDurable"/> is reduced
    /// by its amount for the exited block, and a depleted modifier is removed. Single pass.
    /// </summary>
    public sealed class ExitResolver
    {
        private readonly Board board;

        public ExitResolver(Board board) => this.board = board;

        public void Resolve(Block exited)
        {
            for (int id = 0; id < board.EntityCount; id++)
            {
                Entity entity = board.GetEntity(id);

                if (entity is Block block && block.IsExited)
                    continue;

                WearDown(entity, exited);
            }
        }

        /// <summary>Walks backward so removing a modifier does not shift the ones still to visit.</summary>
        private static void WearDown(Entity entity, Block exited)
        {
            for (int i = entity.ModifierCount - 1; i >= 0; i--)
            {
                if (!(entity.GetModifier(i) is IDurable durable))
                    continue;

                durable.Durability.Reduce(durable.AmountFor(exited));

                if (durable.Durability.IsDepleted)
                    entity.RemoveModifierAt(i);
            }
        }
    }
}
