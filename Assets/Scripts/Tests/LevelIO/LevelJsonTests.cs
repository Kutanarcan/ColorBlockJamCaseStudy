using System;
using Game.Core;
using Game.LevelIO;
using NUnit.Framework;

namespace Game.Tests.LevelIO
{
    public class LevelJsonTests
    {
        private static readonly LevelJson Json =
            new LevelJson(ModifierCatalog.Default().Add(() => new FakeTimedData()));

        // The format documented in the plan (§10).
        private const string DocumentedLevel = @"{
            ""schemaVersion"": 1,
            ""width"": 8, ""height"": 10,
            ""timeLimit"": 90,
            ""walls"":  [ { ""cells"": [[0,0],[1,0],[2,0]] } ],
            ""doors"":  [ { ""colorId"": 2, ""direction"": ""Down"", ""cells"": [[3,0],[4,0]] } ],
            ""blocks"": [
                { ""colorId"": 2, ""cells"": [[3,1],[4,1]],
                  ""modifiers"": [ { ""type"": ""ice"", ""count"": 3 }, { ""type"": ""arrow"", ""direction"": ""Down"" } ] }
            ]
        }";

        private static LevelData SampleLevel() => new LevelData
        {
            SchemaVersion = 1,
            Width = 5,
            Height = 3,
            TimeLimit = 42.5f,
            Walls = new[] { new WallData { Cells = new[] { new Cell(0, 0), new Cell(1, 0) } } },
            Doors = new[] { new DoorData { ColorId = 2, Direction = Direction.Down, Cells = new[] { new Cell(2, 0) } } },
            Blocks = new[]
            {
                new BlockData
                {
                    ColorId = 2,
                    Cells = new[] { new Cell(2, 1), new Cell(3, 1) },
                    Modifiers = new ModifierData[]
                    {
                        new IceData { Count = 3 },
                        new ArrowData { Direction = Direction.Down },
                        new FakeTimedData { Seconds = 7, Facing = Direction.Left }
                    }
                }
            }
        };

        private static LevelData RoundTrip(LevelData level) => Json.Parse(Json.Serialize(level)).Value;

        private static string ModifiersJson(string modifiers) =>
            @"{ ""schemaVersion"": 1, ""width"": 3, ""height"": 3, ""timeLimit"": 60,
                ""blocks"": [ { ""colorId"": 0, ""cells"": [[1,1]], ""modifiers"": [ " + modifiers + @" ] } ] }";

        [Test]
        public void RoundTrip_PreservesAllModifiers()
        {
            ModifierData[] modifiers = RoundTrip(SampleLevel()).Blocks[0].Modifiers;

            Assert.That(modifiers.Length, Is.EqualTo(3));
            Assert.That(((IceData)modifiers[0]).Count, Is.EqualTo(3));
            Assert.That(((ArrowData)modifiers[1]).Direction, Is.EqualTo(Direction.Down));

            var fake = (FakeTimedData)modifiers[2];
            Assert.That(fake.Seconds, Is.EqualTo(7));
            Assert.That(fake.Facing, Is.EqualTo(Direction.Left));
        }

        [Test]
        public void RoundTrip_PreservesGridAndEntities()
        {
            LevelData level = RoundTrip(SampleLevel());

            Assert.That(level.SchemaVersion, Is.EqualTo(1));
            Assert.That(level.Width, Is.EqualTo(5));
            Assert.That(level.Height, Is.EqualTo(3));
            Assert.That(level.TimeLimit, Is.EqualTo(42.5f));
            Assert.That(level.Walls[0].Cells, Is.EqualTo(new[] { new Cell(0, 0), new Cell(1, 0) }));
            Assert.That(level.Doors[0].ColorId, Is.EqualTo(2));
            Assert.That(level.Doors[0].Direction, Is.EqualTo(Direction.Down));
            Assert.That(level.Doors[0].Cells, Is.EqualTo(new[] { new Cell(2, 0) }));
            Assert.That(level.Blocks[0].ColorId, Is.EqualTo(2));
            Assert.That(level.Blocks[0].Cells, Is.EqualTo(new[] { new Cell(2, 1), new Cell(3, 1) }));
        }

        [Test]
        public void RoundTrip_IsStable()
        {
            string json = Json.Serialize(SampleLevel());

            Assert.That(Json.Serialize(Json.Parse(json).Value), Is.EqualTo(json));
        }

        [Test]
        public void Parse_ReadsTheDocumentedFormat()
        {
            LevelData level = Json.Parse(DocumentedLevel).Value;

            Assert.That(level.Width, Is.EqualTo(8));
            Assert.That(level.Doors[0].Direction, Is.EqualTo(Direction.Down));
            Assert.That(level.Doors[0].Cells, Is.EqualTo(new[] { new Cell(3, 0), new Cell(4, 0) }));
            Assert.That(((IceData)level.Blocks[0].Modifiers[0]).Count, Is.EqualTo(3));
            Assert.That(((ArrowData)level.Blocks[0].Modifiers[1]).Direction, Is.EqualTo(Direction.Down));
        }

        [Test]
        public void Parse_Rejects_UnknownModifierType()
        {
            Result<LevelData> result = Json.Parse(ModifiersJson(@"{ ""type"": ""laser"" }"));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("laser"));
        }

        [Test]
        public void Parse_Rejects_ModifierMissingAField_AndNamesWhere()
        {
            Result<LevelData> result = Json.Parse(ModifiersJson(@"{ ""type"": ""ice"" }"));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Does.Contain("level.blocks[0].modifiers[0]").And.Contain("count"));
        }

        [Test]
        public void Parse_Rejects_MissingScalarField()
        {
            Assert.That(Json.Parse(@"{ ""schemaVersion"": 1, ""width"": 3, ""height"": 3 }").IsFailure, Is.True);
        }

        [Test]
        public void Parse_Rejects_ModifierWithoutType()
        {
            Assert.That(Json.Parse(ModifiersJson(@"{ ""count"": 3 }")).IsFailure, Is.True);
        }

        [Test]
        public void Parse_Rejects_NumericDirection()
        {
            Assert.That(Json.Parse(ModifiersJson(@"{ ""type"": ""arrow"", ""direction"": 1 }")).IsFailure, Is.True);
        }

        [Test]
        public void Parse_Rejects_CellThatIsNotAPair()
        {
            Result<LevelData> result = Json.Parse(@"{ ""schemaVersion"": 1, ""width"": 3, ""height"": 3, ""timeLimit"": 60,
                ""walls"": [ { ""cells"": [[1]] } ] }");

            Assert.That(result.IsFailure, Is.True);
        }

        [Test]
        public void Parse_Rejects_MalformedJson()
        {
            Assert.That(Json.Parse(@"{ ""width"": ").IsFailure, Is.True);
        }

        [Test]
        public void Parse_LeavesStructuralChecks_ToTryCreate()
        {
            Result<LevelData> parsed = Json.Parse(@"{ ""schemaVersion"": 0, ""width"": 3, ""height"": 3, ""timeLimit"": 60 }");

            Assert.That(parsed.IsSuccess, Is.True);
            Assert.That(LevelSession.TryCreate(parsed.Value).IsFailure, Is.True);
        }

        [Test]
        public void Serialize_ModifierNotInCatalog_Throws()
        {
            LevelData level = SampleLevel();
            level.Blocks[0].Modifiers = new ModifierData[] { new FakeUnsavableData() };

            Assert.Throws<InvalidOperationException>(() => Json.Serialize(level));
        }
    }
}
