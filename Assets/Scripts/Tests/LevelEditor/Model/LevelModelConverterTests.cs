using Game.Core;
using Game.LevelEditor;
using Game.LevelIO;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelModelConverterTests
    {
        private static readonly LevelJson Json = new LevelJson(ModifierCatalog.Default());

        private static LevelData SampleLevel() => new LevelData
        {
            SchemaVersion = LevelValidator.SupportedSchemaVersion,
            Width = 4,
            Height = 3,
            TimeLimit = 30f,
            Walls = new[]
            {
                new WallData { Cells = new[] { new Cell(0, 0), new Cell(3, 0), new Cell(0, 1), new Cell(3, 1) } },
                new WallData { Cells = new[] { new Cell(0, 2), new Cell(1, 2), new Cell(2, 2), new Cell(3, 2) } }
            },
            Doors = new[] { new DoorData { ColorId = 1, Direction = Direction.Down, Cells = new[] { new Cell(1, 0), new Cell(2, 0) } } },
            Blocks = new[]
            {
                new BlockData
                {
                    ColorId = 1,
                    Cells = new[] { new Cell(1, 1), new Cell(2, 1) },
                    Modifiers = new ModifierData[] { new IceData { Count = 2 }, new ArrowData { Axis = Axis.Vertical } }
                }
            }
        };

        [Test]
        public void EditorModel_RoundTrip_EqualsLevelData()
        {
            LevelData original = SampleLevel();

            LevelModel model = LevelModelConverter.FromLevelData(original).Value;
            LevelData saved = LevelModelConverter.ToLevelData(model);

            Assert.That(Json.Serialize(saved), Is.EqualTo(Json.Serialize(original)));
        }

        [Test]
        public void FromLevelData_Rejects_OverlappingEntities()
        {
            LevelData level = SampleLevel();
            level.Blocks[0].Cells = new[] { new Cell(0, 0) };

            Assert.That(LevelModelConverter.FromLevelData(level).IsFailure, Is.True);
        }

        [Test]
        public void FromLevelData_Accepts_InvalidModifierValues_SoTheyCanBeFixed()
        {
            LevelData level = SampleLevel();
            level.Blocks[0].Modifiers = new ModifierData[] { new IceData { Count = 0 } };

            Assert.That(LevelModelConverter.FromLevelData(level).IsSuccess, Is.True);
        }

        [Test]
        public void ToLevelData_SkipsErasedEntities()
        {
            LevelModel model = LevelModelConverter.FromLevelData(SampleLevel()).Value;
            model.Erase(new Cell(1, 1));
            model.Erase(new Cell(2, 1));

            Assert.That(LevelModelConverter.ToLevelData(model).Blocks, Is.Empty);
        }
    }
}
