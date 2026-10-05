using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class RopeTests
    {
        private static readonly Cell RCell = new Cell(1, 1);
        private static readonly Cell SCell = new Cell(4, 1);

        // R can step right into a free cell; S sits above a door of its color.
        private static LevelSession NewSession(ModifierData[] r, ModifierData[] s) =>
            new LevelSession(AsciiLevel.Parse(
                    "######",
                    "#R..S#",
                    "####1#")
                .Block('R', 1, r)
                .Block('S', 0, s)
                .Door('1', 0, Direction.Down)
                .Build());

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        private static ModifierData[] Mods(params ModifierData[] modifiers) => modifiers;

        [Test]
        public void Rope_IsCut_WhenMatchingScissorsExit()
        {
            LevelSession session = NewSession(
                Mods(new RopeData { ColorId = 3 }),
                Mods(new ScissorsData { ColorId = 3 }));
            Block r = BlockAt(session, RCell);
            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            session.TryMove(BlockAt(session, SCell), Direction.Down);

            Assert.That(session.TryMove(r, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void Rope_StaysOn_WhenScissorsColorDiffers()
        {
            LevelSession session = NewSession(
                Mods(new RopeData { ColorId = 3 }),
                Mods(new ScissorsData { ColorId = 4 }));

            session.TryMove(BlockAt(session, SCell), Direction.Down);

            Assert.That(session.TryMove(BlockAt(session, RCell), Direction.Right), Is.EqualTo(MoveResult.Blocked));
        }

        [Test]
        public void EveryRopeColor_NeedsItsOwnScissors()
        {
            LevelSession session = NewSession(
                Mods(new RopeData { ColorId = 3 }, new RopeData { ColorId = 4 }),
                Mods(new ScissorsData { ColorId = 3 }));

            session.TryMove(BlockAt(session, SCell), Direction.Down);

            Assert.That(session.TryMove(BlockAt(session, RCell), Direction.Right), Is.EqualTo(MoveResult.Blocked));
        }
    }
}
