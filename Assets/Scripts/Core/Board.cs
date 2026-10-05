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
                Write(entities[i]);
        }

        public Entity GetEntity(int id) => entities[id];

        /// <returns>The occupant of <paramref name="cell"/>, or null when it is empty.</returns>
        public Entity EntityAt(Cell cell)
        {
            int id = Grid.Get(cell);
            return id == Grid.Empty ? null : entities[id];
        }

        private void Write(Entity entity)
        {
            for (int i = 0; i < entity.CellCount; i++)
                Grid.Set(entity.GetCell(i), entity.Id);
        }
    }
}
