using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// What the Paint tool lays down: an entity kind with its color and direction. Choosing a brush creates
    /// nothing; a stroke creates the entity when it paints the first cell (D53). A door that starts on the edge
    /// points out of the grid, the only direction the edge allows; the brush's direction is for inner doors (D57).
    /// A struct: three immutable fields, replaced whole when the designer changes one.
    /// </summary>
    public readonly struct Brush
    {
        public EntityKind Kind { get; }
        public int ColorId { get; }
        public Direction Direction { get; }

        public Brush(EntityKind kind, int colorId, Direction direction)
        {
            Kind = kind;
            ColorId = colorId;
            Direction = direction;
        }

        /// <summary>The direction of a door this brush paints starting at <paramref name="cell"/>.</summary>
        public Direction DirectionAt(LevelModel model, Cell cell) =>
            EdgeCells.IsEdge(model, cell) && !EdgeCells.IsCorner(model, cell) ? EdgeCells.Outward(model, cell) : Direction;

        /// <summary>True when a stroke starting at <paramref name="cell"/> on this entity extends it instead of painting a new one.</summary>
        public bool Matches(LevelModel model, int entity, Cell cell)
        {
            if (model.KindOf(entity) != Kind)
                return false;

            switch (Kind)
            {
                case EntityKind.Wall: return true;
                case EntityKind.Door: return model.ColorOf(entity) == ColorId && model.DirectionOf(entity) == DirectionAt(model, cell);
                default: return model.ColorOf(entity) == ColorId;
            }
        }

        /// <summary>Adds the entity a stroke paints, its first cell being <paramref name="cell"/>.</summary>
        public int CreateIn(LevelModel model, Cell cell)
        {
            switch (Kind)
            {
                case EntityKind.Wall: return model.AddWall();
                case EntityKind.Door: return model.AddDoor(ColorId, DirectionAt(model, cell));
                default: return model.AddBlock(ColorId);
            }
        }
    }
}
