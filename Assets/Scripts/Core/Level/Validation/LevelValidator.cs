using System.Collections.Generic;

namespace Game.Core
{
    public static class LevelValidator
    {
        public const int SupportedSchemaVersion = 2;

        public static IReadOnlyList<LevelError> Validate(LevelData level)
        {
            var errors = new List<LevelError>();

            if (level.SchemaVersion != SupportedSchemaVersion)
            {
                errors.Add(new LevelError(LevelErrorKind.UnsupportedSchemaVersion,
                    $"Version {level.SchemaVersion}, supported {SupportedSchemaVersion}"));

                return errors;
            }

            if (level.Width <= 0 || level.Height <= 0)
            {
                errors.Add(new LevelError(LevelErrorKind.InvalidGridSize, $"{level.Width} x {level.Height}"));

                return errors;
            }

            var owners = new string[level.Width * level.Height];

            for (int i = 0; i < level.Walls.Length; i++)
            {
                CheckCells(level, level.Walls[i].Cells, $"Wall {i}", owners, errors);
            }

            for (int i = 0; i < level.Doors.Length; i++)
            {
                CheckCells(level, level.Doors[i].Cells, $"Door {i}", owners, errors);
            }

            for (int i = 0; i < level.Blocks.Length; i++)
            {
                string label = $"Block {i}";
                CheckCells(level, level.Blocks[i].Cells, label, owners, errors);
                CheckModifiers(level.Blocks[i].Modifiers, label, errors);
            }

            return errors;
        }

        private static void CheckCells(LevelData level, Cell[] cells, string label, string[] owners,
            List<LevelError> errors)
        {
            if (cells == null || cells.Length == 0)
            {
                errors.Add(new LevelError(LevelErrorKind.EmptyEntity, label));

                return;
            }

            foreach (Cell cell in cells)
            {
                if (cell.X < 0 || cell.X >= level.Width || cell.Y < 0 || cell.Y >= level.Height)
                {
                    errors.Add(new LevelError(LevelErrorKind.OutOfBounds, $"{label} at {cell}"));
                    continue;
                }

                int index = cell.Y * level.Width + cell.X;

                if (owners[index] != null)
                    errors.Add(new LevelError(LevelErrorKind.Overlap, $"{label} and {owners[index]} at {cell}"));
                else
                    owners[index] = label;
            }
        }

        private static void CheckModifiers(ModifierData[] modifiers, string label, List<LevelError> errors)
        {
            if (modifiers == null)
            {
                errors.Add(new LevelError(LevelErrorKind.InvalidModifier, $"{label}: modifier list is missing"));

                return;
            }

            for (int i = 0; i < modifiers.Length; i++)
            {
                Result result = modifiers[i] == null ? Result.Failure("is missing") : modifiers[i].Validate();

                if (result.IsFailure)
                    errors.Add(new LevelError(LevelErrorKind.InvalidModifier, $"{label}: modifier {i} {result.Error}"));
            }
        }
    }
}
