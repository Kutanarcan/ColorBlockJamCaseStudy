using System.Collections.Generic;
using Game.Core;
using Game.LevelIO;
using NUnit.Framework;

namespace Game.Tests.Additivity
{
    /// <summary>
    /// Phase 13: a mechanic from the reference added end to end (data, JSON, logic) from an assembly that sees only
    /// the public API of Game.Core and Game.LevelIO. Registering it is one <see cref="ModifierCatalog.Add"/> call.
    /// </summary>
    public class TurnBasedArrowTests
    {
        private static readonly Cell RCell = new Cell(2, 1);
        private static readonly Cell ACell = new Cell(5, 1);
        private static readonly Cell BCell = new Cell(6, 1);

        // R carries the modifier and has free cells on both sides. A and B each sit above a door of their color,
        // so two exits are available.
        private static readonly string[] Map =
        {
            "########",
            "#.R..AB#",
            "#####12#"
        };

        private static ModifierCatalog Catalog() => ModifierCatalog.Default().Add(() => new TurnBasedArrowData());

        private static TurnBasedArrowData LockedRight(int count) =>
            new TurnBasedArrowData { Direction = Direction.Right, Count = count };

        /// <summary>The level saved to JSON and loaded back, the way a level file reaches the game.</summary>
        private static LevelSession Load(params ModifierData[] rModifiers)
        {
            var json = new LevelJson(Catalog());
            Result<LevelData> parsed = json.Parse(json.Serialize(Level(rModifiers)));

            return LevelSession.TryCreate(parsed.Value).Value;
        }

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        private static void Exit(LevelSession session, Cell cell) =>
            Assert.That(session.TryMove(BlockAt(session, cell), Direction.Down), Is.EqualTo(MoveResult.Exited));

        [Test]
        public void TurnBasedArrow_AddedWithoutCoreChanges()
        {
            LevelSession session = Load(LockedRight(2));
            Block r = BlockAt(session, RCell);

            Assert.That(session.TryMove(r, Direction.Left), Is.EqualTo(MoveResult.Blocked));
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Moved));

            Exit(session, ACell);
            Assert.That(session.TryMove(r, Direction.Left), Is.EqualTo(MoveResult.Blocked));

            Exit(session, BCell);
            Assert.That(r.ModifierCount, Is.EqualTo(0));
            Assert.That(session.TryMove(r, Direction.Left), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void RoundTrip_KeepsDirectionAndCount()
        {
            var json = new LevelJson(Catalog());

            LevelData loaded = json.Parse(json.Serialize(Level(LockedRight(3)))).Value;

            var data = (TurnBasedArrowData)loaded.Blocks[0].Modifiers[0];
            Assert.That(data.Direction, Is.EqualTo(Direction.Right));
            Assert.That(data.Count, Is.EqualTo(3));
        }

        [Test]
        public void WithoutItsCatalogLine_ALevelUsingItIsRejected()
        {
            string text = new LevelJson(Catalog()).Serialize(Level(LockedRight(1)));

            Assert.That(new LevelJson(ModifierCatalog.Default()).Parse(text).IsFailure, Is.True);
        }

        [Test]
        public void ItsOwnValueCheck_RejectsTheLevelAtLoad()
        {
            Assert.That(LevelSession.TryCreate(Level(LockedRight(0))).IsFailure, Is.True);
        }

        [Test]
        public void StacksWithIce_FrozenFirst_ThenLocked_ThenFree()
        {
            LevelSession session = Load(new IceData { Count = 1 }, LockedRight(2));
            Block r = BlockAt(session, RCell);
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            Exit(session, ACell);
            Assert.That(session.TryMove(r, Direction.Left), Is.EqualTo(MoveResult.Blocked));
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Moved));

            Exit(session, BCell);
            Assert.That(session.TryMove(r, Direction.Left), Is.EqualTo(MoveResult.Moved));
        }

        /// <summary>
        /// <see cref="Map"/> as LevelData: '#' walls, 'R' (color 2, given modifiers), 'A' (color 0) and 'B' (color 1)
        /// blocks, '1' / '2' doors of colors 0 / 1 facing down. The first row is the top.
        /// </summary>
        private static LevelData Level(params ModifierData[] rModifiers)
        {
            var walls = new List<Cell>();
            int height = Map.Length;

            for (int row = 0; row < height; row++)
            {
                for (int x = 0; x < Map[row].Length; x++)
                {
                    if (Map[row][x] == '#')
                        walls.Add(new Cell(x, height - 1 - row));
                }
            }

            return new LevelData
            {
                SchemaVersion = LevelValidator.SupportedSchemaVersion,
                Width = Map[0].Length,
                Height = height,
                TimeLimit = 60f,
                Walls = new[] { new WallData { Cells = walls.ToArray() } },
                Doors = new[] { Door(0, new Cell(5, 0)), Door(1, new Cell(6, 0)) },
                Blocks = new[]
                {
                    new BlockData { ColorId = 2, Cells = new[] { RCell }, Modifiers = rModifiers },
                    new BlockData { ColorId = 0, Cells = new[] { ACell } },
                    new BlockData { ColorId = 1, Cells = new[] { BCell } }
                }
            };
        }

        private static DoorData Door(int colorId, Cell cell) =>
            new DoorData { ColorId = colorId, Direction = Direction.Down, Cells = new[] { cell } };
    }
}
