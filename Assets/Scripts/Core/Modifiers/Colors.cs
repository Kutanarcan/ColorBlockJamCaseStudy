namespace Game.Core
{
    /// <summary>
    /// The one place colors are read. A color source modifier overrides the base color;
    /// the most recently added one wins. Nothing else reads base colors directly.
    /// </summary>
    public static class Colors
    {
        public const int None = -1;

        public static int Of(Entity entity)
        {
            for (int i = entity.ModifierCount - 1; i >= 0; i--)
            {
                if (entity.GetModifier(i) is IColorSource source)
                    return source.ColorId;
            }

            return BaseOf(entity);
        }

        private static int BaseOf(Entity entity)
        {
            if (entity is Block block)
                return block.BaseColorId;

            if (entity is Door door)
                return door.BaseColorId;

            return None;
        }
    }
}
