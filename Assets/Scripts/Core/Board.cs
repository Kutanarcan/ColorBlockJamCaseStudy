namespace Game.Core
{
    /// <summary>The entity table plus the occupancy grid they are written into.</summary>
    public sealed class Board
    {
        private readonly Entity[] entities;

        public Grid Grid { get; }
        public int EntityCount => entities.Length;

        /// <param name="entities">Indexed by id: <c>entities[i].Id == i</c>.</param>
        public Board(int width, int height, Entity[] entities)
        {
            Grid = new Grid(width, height);
            this.entities = entities;

            for (int i = 0; i < entities.Length; i++)
                Fill(entities[i], entities[i].Id);
        }

        public Entity GetEntity(int id) => entities[id];

        /// <returns>The occupant of <paramref name="cell"/>, or null when it is empty.</returns>
        public Entity EntityAt(Cell cell)
        {
            int id = Grid.Get(cell);
            return id == Grid.Empty ? null : entities[id];
        }

        /// <summary>True when every cell the entity would cover after <paramref name="offset"/> is empty or its own.</summary>
        public bool CanPlace(Entity entity, Cell offset)
        {
            for (int i = 0; i < entity.CellCount; i++)
            {
                int occupant = Grid.Get(entity.GetCell(i) + offset);

                if (occupant != Grid.Empty && occupant != entity.Id)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Clears every old cell first, then writes every new one. Clearing and writing cell by cell
        /// would erase cells the entity still covers after the move.
        /// </summary>
        internal void MoveEntity(Entity entity, Cell offset)
        {
            Fill(entity, Grid.Empty);
            entity.Position += offset;
            Fill(entity, entity.Id);
        }

        /// <summary>Clears all of the block's cells at once. Its position keeps the last cell it stood on.</summary>
        internal void RemoveBlock(Block block)
        {
            Fill(block, Grid.Empty);
            block.IsExited = true;
        }

        private void Fill(Entity entity, int value)
        {
            for (int i = 0; i < entity.CellCount; i++)
                Grid.Set(entity.GetCell(i), value);
        }
    }
}
