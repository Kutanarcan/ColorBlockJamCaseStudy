using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class BoardBuilderTests
    {
        private static LevelData SampleLevel() =>
            AsciiLevel.Parse(
                    "#####",
                    "#AA.#",
                    "#.B.#",
                    "##1##")
                .Block('A', 0)
                .Block('B', 1)
                .Door('1', 0, Direction.Down)
                .Build();

        [Test]
        public void Builder_PlacesEntities_IntoOccupancy()
        {
            Board board = BoardBuilder.Build(SampleLevel());

            Assert.That(board.EntityAt(new Cell(0, 0)), Is.InstanceOf<Wall>());
            Assert.That(board.EntityAt(new Cell(2, 0)), Is.InstanceOf<Door>());
            Assert.That(board.EntityAt(new Cell(3, 2)), Is.Null);

            var a = (Block)board.EntityAt(new Cell(1, 2));
            Assert.That(a.ColorId, Is.EqualTo(0));
            Assert.That(board.EntityAt(new Cell(2, 2)), Is.SameAs(a));

            var b = (Block)board.EntityAt(new Cell(2, 1));
            Assert.That(b.ColorId, Is.EqualTo(1));
        }

        [Test]
        public void Builder_StoresShape_RelativeToMinCorner()
        {
            Board board = BoardBuilder.Build(SampleLevel());
            Entity a = board.EntityAt(new Cell(1, 2));

            Assert.That(a.Position, Is.EqualTo(new Cell(1, 2)));
            Assert.That(a.GetCell(0), Is.EqualTo(new Cell(1, 2)));
            Assert.That(a.GetCell(1), Is.EqualTo(new Cell(2, 2)));
        }

        [Test]
        public void Builder_KeepsDoorColorAndDirection()
        {
            Board board = BoardBuilder.Build(SampleLevel());
            var door = (Door)board.EntityAt(new Cell(2, 0));

            Assert.That(door.ColorId, Is.EqualTo(0));
            Assert.That(door.Direction, Is.EqualTo(Direction.Down));
        }

        [Test]
        public void Builder_AssignsIds_MatchingEntityTable()
        {
            Board board = BoardBuilder.Build(SampleLevel());

            for (int id = 0; id < board.EntityCount; id++)
                Assert.That(board.GetEntity(id).Id, Is.EqualTo(id));
        }

        [Test]
        public void Builder_CopiesCells_LeavingLevelDataUntouched()
        {
            LevelData level = SampleLevel();
            Board board = BoardBuilder.Build(level);

            level.Blocks[0].Cells[0] = new Cell(3, 1);

            Assert.That(board.EntityAt(new Cell(1, 2)), Is.InstanceOf<Block>());
            Assert.That(board.EntityAt(new Cell(3, 1)), Is.Null);
        }
    }
}
