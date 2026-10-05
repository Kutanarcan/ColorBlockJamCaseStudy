namespace Game.Core
{
    public sealed class Board
    {
        private readonly Entity[] entities;

        public Grid Grid { get; }
        public int EntityCount => entities.Length;

        public Board(int width, int height, Entity[] entities)
        {
            Grid = new Grid(width, height);
            this.entities = entities;

            for (int i = 0; i < entities.Length; i++)
                Fill(entities[i], entities[i].Id);
        }

        public Entity GetEntity(int id) => entities[id];

        public Entity EntityAt(Cell cell)
        {
            int id = Grid.Get(cell);
            return id == Grid.Empty ? null : entities[id];
        }

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

        internal void MoveEntity(Entity entity, Cell offset)
        {
            Fill(entity, Grid.Empty);
            entity.Position += offset;
            Fill(entity, entity.Id);
        }

        internal void RemoveBlock(Block block)
        {
            Fill(block, Grid.Empty);
            block.IsExited = true;
        }

        private void Fill(Entity entity, int value)
        {
            for (int i = 0; i < entity.CellCount; i++)
            {
                Grid.Set(entity.GetCell(i), value);
            }
        }
    }
}
