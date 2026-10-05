namespace Game.Core
{
    /// <summary>
    /// Turns a <see cref="LevelData"/> into a fresh <see cref="Board"/>.
    /// Ids are assigned in order walls, doors, blocks. Cells are copied and modifiers are created fresh,
    /// so the definition stays untouched and every build starts with full counters.
    /// </summary>
    public static class BoardBuilder
    {
        public static Board Build(LevelData level)
        {
            var entities = new Entity[level.Walls.Length + level.Doors.Length + level.Blocks.Length];
            int id = 0;

            foreach (WallData wall in level.Walls)
            {
                Cell origin = MinCorner(wall.Cells);
                entities[id] = new Wall(id, origin, ToShape(wall.Cells, origin));
                id++;
            }

            foreach (DoorData door in level.Doors)
            {
                Cell origin = MinCorner(door.Cells);
                entities[id] = new Door(id, origin, ToShape(door.Cells, origin), door.ColorId, door.Direction);
                id++;
            }

            foreach (BlockData block in level.Blocks)
            {
                Cell origin = MinCorner(block.Cells);
                entities[id] = new Block(id, origin, ToShape(block.Cells, origin), block.ColorId,
                    ToModifiers(block.Modifiers));
                id++;
            }

            return new Board(level.Width, level.Height, entities);
        }

        private static Cell MinCorner(Cell[] cells)
        {
            int minX = cells[0].X;
            int minY = cells[0].Y;
            for (int i = 1; i < cells.Length; i++)
            {
                if (cells[i].X < minX) minX = cells[i].X;
                if (cells[i].Y < minY) minY = cells[i].Y;
            }
            return new Cell(minX, minY);
        }

        private static Cell[] ToShape(Cell[] cells, Cell origin)
        {
            var shape = new Cell[cells.Length];
            for (int i = 0; i < cells.Length; i++)
                shape[i] = cells[i] - origin;
            return shape;
        }

        private static IModifier[] ToModifiers(ModifierData[] data)
        {
            var modifiers = new IModifier[data.Length];
            for (int i = 0; i < data.Length; i++)
                modifiers[i] = data[i].ToModifier();
            return modifiers;
        }
    }
}
