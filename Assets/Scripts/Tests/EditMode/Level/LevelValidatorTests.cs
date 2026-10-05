using System;
using System.Collections.Generic;
using System.Linq;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelValidatorTests
    {
        // Walls fill row 0 and the sides; A sits above the door at (2, 0).
        private static LevelData ValidLevel() =>
            AsciiLevel.Parse(
                    "#####",
                    "#A..#",
                    "##1##")
                .Block('A', 0)
                .Door('1', 0, Direction.Down)
                .Build();

        private static LevelData WithBlock(LevelData level, ModifierData[] modifiers, params Cell[] cells)
        {
            var block = new BlockData { ColorId = 0, Cells = cells, Modifiers = modifiers };
            level.Blocks = level.Blocks.Append(block).ToArray();

            return level;
        }

        private static LevelErrorKind[] KindsOf(IReadOnlyList<LevelError> errors) =>
            errors.Select(e => e.Kind).ToArray();

        [Test]
        public void ValidLevel_HasNoErrors()
        {
            Assert.That(LevelValidator.Validate(ValidLevel()), Is.Empty);
        }

        [Test]
        public void Load_Rejects_OverlappingEntities()
        {
            LevelData level = WithBlock(ValidLevel(), Array.Empty<ModifierData>(), new Cell(1, 1));

            Assert.That(KindsOf(LevelValidator.Validate(level)), Is.EqualTo(new[] { LevelErrorKind.Overlap }));
        }

        [Test]
        public void Rejects_CellOutsideTheGrid()
        {
            LevelData level = WithBlock(ValidLevel(), Array.Empty<ModifierData>(), new Cell(5, 1));

            Assert.That(KindsOf(LevelValidator.Validate(level)), Is.EqualTo(new[] { LevelErrorKind.OutOfBounds }));
        }

        [Test]
        public void Rejects_EntityWithoutCells()
        {
            LevelData level = WithBlock(ValidLevel(), Array.Empty<ModifierData>());

            Assert.That(KindsOf(LevelValidator.Validate(level)), Is.EqualTo(new[] { LevelErrorKind.EmptyEntity }));
        }

        [Test]
        public void Rejects_UnsupportedSchemaVersion()
        {
            LevelData level = ValidLevel();
            level.SchemaVersion = LevelValidator.SupportedSchemaVersion + 1;

            Assert.That(KindsOf(LevelValidator.Validate(level)),
                Is.EqualTo(new[] { LevelErrorKind.UnsupportedSchemaVersion }));
        }

        [Test]
        public void Rejects_NonPositiveGridSize()
        {
            LevelData level = ValidLevel();
            level.Width = 0;

            Assert.That(KindsOf(LevelValidator.Validate(level)), Is.EqualTo(new[] { LevelErrorKind.InvalidGridSize }));
        }

        [Test]
        public void Rejects_IceWithZeroCount()
        {
            LevelData level = WithBlock(ValidLevel(), new ModifierData[] { new IceData { Count = 0 } }, new Cell(3, 1));

            Assert.That(KindsOf(LevelValidator.Validate(level)), Is.EqualTo(new[] { LevelErrorKind.InvalidModifier }));
        }

        [Test]
        public void Rejects_MissingModifierEntry()
        {
            LevelData level = WithBlock(ValidLevel(), new ModifierData[] { null }, new Cell(3, 1));

            Assert.That(KindsOf(LevelValidator.Validate(level)), Is.EqualTo(new[] { LevelErrorKind.InvalidModifier }));
        }

        [Test]
        public void Reports_EveryError_NotOnlyTheFirst()
        {
            LevelData level = WithBlock(ValidLevel(), new ModifierData[] { new IceData { Count = -1 } },
                new Cell(1, 1), new Cell(9, 9));

            Assert.That(KindsOf(LevelValidator.Validate(level)), Is.EquivalentTo(new[]
            {
                LevelErrorKind.Overlap, LevelErrorKind.OutOfBounds, LevelErrorKind.InvalidModifier
            }));
        }
    }
}
