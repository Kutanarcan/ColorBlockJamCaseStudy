using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>LevelData ↔ LevelModel, on load and save only. Entities are written walls, doors, blocks, like the builder.</summary>
    public static class LevelModelConverter
    {
        /// <summary>
        /// Fails when the data cannot live on a grid (bad size, cells outside it, overlaps). Wrong modifier values
        /// are accepted, so the designer can open the level and fix them; the rules report them.
        /// </summary>
        public static Result<LevelModel> FromLevelData(LevelData level)
        {
            Result blocking = BlockingErrors(level);

            if (blocking.IsFailure)
                return Result<LevelModel>.Failure(blocking.Error);

            var model = new LevelModel(level.Width, level.Height) { TimeLimit = level.TimeLimit };

            foreach (WallData wall in level.Walls)
            {
                PaintAll(model, model.AddWall(), wall.Cells);
            }

            foreach (DoorData door in level.Doors)
            {
                PaintAll(model, model.AddDoor(door.ColorId, door.Direction), door.Cells);
            }

            foreach (BlockData block in level.Blocks)
            {
                int id = model.AddBlock(block.ColorId);
                PaintAll(model, id, block.Cells);

                foreach (ModifierData modifier in block.Modifiers)
                {
                    model.AddModifier(id, modifier);
                }
            }

            return Result<LevelModel>.Success(model);
        }

        public static LevelData ToLevelData(LevelModel model)
        {
            var ids = new List<int>();

            return new LevelData
            {
                SchemaVersion = LevelValidator.SupportedSchemaVersion,
                Width = model.Width,
                Height = model.Height,
                TimeLimit = model.TimeLimit,
                Walls = ToWalls(model, ids),
                Doors = ToDoors(model, ids),
                Blocks = ToBlocks(model, ids)
            };
        }

        private static WallData[] ToWalls(LevelModel model, List<int> ids)
        {
            model.CollectEntities(EntityKind.Wall, ids);
            var walls = new WallData[ids.Count];

            for (int i = 0; i < ids.Count; i++)
            {
                walls[i] = new WallData { Cells = CellsOf(model, ids[i]) };
            }

            return walls;
        }

        private static DoorData[] ToDoors(LevelModel model, List<int> ids)
        {
            model.CollectEntities(EntityKind.Door, ids);
            var doors = new DoorData[ids.Count];

            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                doors[i] = new DoorData
                {
                    ColorId = model.ColorOf(id), Direction = model.DirectionOf(id), Cells = CellsOf(model, id)
                };
            }

            return doors;
        }

        private static BlockData[] ToBlocks(LevelModel model, List<int> ids)
        {
            model.CollectEntities(EntityKind.Block, ids);
            var blocks = new BlockData[ids.Count];

            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                blocks[i] = new BlockData
                {
                    ColorId = model.ColorOf(id), Cells = CellsOf(model, id), Modifiers = ModifiersOf(model, id)
                };
            }

            return blocks;
        }

        private static Result BlockingErrors(LevelData level)
        {
            foreach (LevelError error in LevelValidator.Validate(level))
            {
                if (error.Kind != LevelErrorKind.InvalidModifier)
                    return Result.Failure(error.ToString());
            }

            return Result.Success();
        }

        private static void PaintAll(LevelModel model, int entity, Cell[] cells)
        {
            foreach (Cell cell in cells)
            {
                model.Paint(cell, entity);
            }
        }

        private static Cell[] CellsOf(LevelModel model, int entity)
        {
            IReadOnlyList<int> indices = model.CellsOf(entity);
            var cells = new Cell[indices.Count];

            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = model.CellOf(indices[i]);
            }

            return cells;
        }

        private static ModifierData[] ModifiersOf(LevelModel model, int entity)
        {
            IReadOnlyList<ModifierData> source = model.ModifiersOf(entity);

            if (source.Count == 0)
                return Array.Empty<ModifierData>();

            var result = new ModifierData[source.Count];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = source[i];
            }

            return result;
        }
    }
}
