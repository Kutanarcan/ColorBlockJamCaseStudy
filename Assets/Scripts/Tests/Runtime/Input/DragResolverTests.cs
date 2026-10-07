using Game.Core;
using Game.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests.Runtime
{
    public class DragResolverTests
    {
        private static readonly Cell Up = new Cell(0, 1);
        private static readonly Cell Right = new Cell(1, 0);

        /// <summary>A session with one 1×1 block of color 0; doors in the map face left.</summary>
        private static (LevelSession session, Block block, FakeMoveObserver moves) Start(Cell block, params string[] rows) =>
            Start(block, new ModifierData[0], rows);

        private static (LevelSession session, Block block, FakeMoveObserver moves) Start(Cell block,
            ModifierData[] modifiers, params string[] rows)
        {
            LevelData level = LevelMap.Parse(Direction.Left, rows);
            level.TimeLimit = 60f;
            level.Blocks = new[] { new BlockData { ColorId = 0, Cells = new[] { block }, Modifiers = modifiers } };

            LevelSession session = LevelSession.TryCreate(level).Value;
            var moves = new FakeMoveObserver();
            session.Observer = moves;

            // Blocks are built after walls and doors, so the only block is the last entity.
            return (session, (Block)session.Board.GetEntity(session.Board.EntityCount - 1), moves);
        }

        [Test]
        public void DragResolver_MovesTowardThePointer_LargerAxisFirst()
        {
            (LevelSession session, Block block, FakeMoveObserver moves) = Start(new Cell(1, 1),
                "#######",
                "#.....#",
                "#.....#",
                "#.....#",
                "#.....#",
                "#######");

            MoveResult result = new DragResolver(session).MoveToward(block, new Cell(2, 3));

            Assert.That(result, Is.EqualTo(MoveResult.Moved));
            Assert.That(block.Position, Is.EqualTo(new Cell(2, 3)));
            // Remaining (1, 2): Y is larger → up; then (1, 1): a tie goes to X → right; then (0, 1) → up.
            Assert.That(moves.Moves, Is.EqualTo(new[] { Up, Right, Up }));
        }

        [Test]
        public void DragResolver_TakesTheOtherAxis_WhenTheLargerIsBlocked()
        {
            (LevelSession session, Block block, FakeMoveObserver moves) = Start(new Cell(1, 1),
                "#######",
                "#.....#",
                "#.....#",
                "#.....#",
                "#.#...#",
                "#######");

            new DragResolver(session).MoveToward(block, new Cell(3, 2));

            Assert.That(block.Position, Is.EqualTo(new Cell(3, 2)));
            Assert.That(moves.Moves, Is.EqualTo(new[] { Up, Right, Right }), "the wall at (2, 1) sends it up first");
        }

        [Test]
        public void DragResolver_StopsMoving_WhenTheBlockExits()
        {
            (LevelSession session, Block block, _) = Start(new Cell(2, 1),
                "#####",
                "#...#",
                "0...#",
                "#####");

            MoveResult result = new DragResolver(session).MoveToward(block, new Cell(-3, 1));

            Assert.That(result, Is.EqualTo(MoveResult.Exited));
            Assert.That(block.IsExited);
        }

        [Test]
        public void ClampToReachable_GoesAtMostHalfACell_AndNeverTowardARefusedCell()
        {
            (LevelSession session, Block block, _) = Start(new Cell(1, 2),
                "#######",
                "#.....#",
                "#.#...#",
                "#.....#",
                "#.....#",
                "#######");
            var resolver = new DragResolver(session);

            AssertNear(resolver.ClampToReachable(block, new Vector2(1f, 3.4f)), new Vector2(1f, 2.5f), "up is clamped to half a cell");
            AssertNear(resolver.ClampToReachable(block, new Vector2(0.6f, 2.3f)), new Vector2(1f, 2.3f), "the frame refuses left");
            AssertNear(resolver.ClampToReachable(block, new Vector2(1.3f, 2.9f)), new Vector2(1f, 2.5f),
                "the wall at (2, 3) refuses the diagonal; the smaller axis goes");
        }

        [Test]
        public void ClampToReachable_OfAOneWayBlock_FollowsBackToTheCellItJustLeft_ButNotFurther()
        {
            (LevelSession session, Block block, _) = Start(new Cell(3, 1),
                new ModifierData[] { new FakeOneWayData { Direction = Direction.Left } },
                "######",
                "#....#",
                "######");
            var resolver = new DragResolver(session);
            resolver.BeginDrag();

            AssertNear(resolver.ClampToReachable(block, new Vector2(3.4f, 1f)), new Vector2(3f, 1f), "new drag: right is refused");

            resolver.MoveToward(block, new Cell(2, 1));

            AssertNear(resolver.ClampToReachable(block, new Vector2(2.4f, 1f)), new Vector2(2.4f, 1f), "back toward (3, 1), just left");
            AssertNear(resolver.ClampToReachable(block, new Vector2(1.7f, 1f)), new Vector2(1.7f, 1f), "on in its own direction");
        }

        [Test]
        public void DragResolver_ExitsEarly_WhenPulledPastTheThreshold_TowardItsDoor()
        {
            (LevelSession session, Block block, _) = Start(new Cell(1, 1),
                "#####",
                "0...#",
                "#####");
            var resolver = new DragResolver(session);

            Assert.That(resolver.TryExitToward(block, new Vector2(0.8f, 1f), 0.3f), Is.False, "0.2 of a cell: not yet");
            Assert.That(block.IsExited, Is.False);

            Assert.That(resolver.TryExitToward(block, new Vector2(0.6f, 1f), 0.3f), Is.True, "0.4 of a cell: out");
            Assert.That(block.IsExited);
        }

        [Test]
        public void DragResolver_DoesNotExit_TowardAWall()
        {
            (LevelSession session, Block block, _) = Start(new Cell(1, 1),
                "#####",
                "0...#",
                "#####");

            Assert.That(new DragResolver(session).TryExitToward(block, new Vector2(1f, 0.4f), 0.3f), Is.False);
            Assert.That(block.IsExited, Is.False);
        }

        [Test]
        public void ClampToReachable_LeansIntoAnOpenDoor()
        {
            (LevelSession session, Block block, _) = Start(new Cell(1, 1),
                "#####",
                "0...#",
                "#####");

            AssertNear(new DragResolver(session).ClampToReachable(block, new Vector2(0.8f, 1f)), new Vector2(0.8f, 1f),
                "Core says the next move exits, so the block may lean into the door");
        }

        private static void AssertNear(Vector2 actual, Vector2 expected, string message) =>
            Assert.That(Vector2.Distance(actual, expected), Is.LessThan(1e-4f), $"{message}: {actual}");
    }
}
