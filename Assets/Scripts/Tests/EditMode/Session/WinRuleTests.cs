using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class WinRuleTests
    {
        [Test]
        public void ExemptBlock_DoesNotHoldBackTheWin()
        {
            var session = new LevelSession(AsciiLevel.Parse(
                    "######",
                    "#A..T#",
                    "#1####")
                .Block('A', 0)
                .Block('T', 1, new FakeModifierData(() => new FakeWinExempt()))
                .Door('1', 0, Direction.Down)
                .Build());

            session.TryMove((Block)session.Board.EntityAt(new Cell(1, 1)), Direction.Down);

            Assert.That(session.State, Is.EqualTo(GameState.Won));
        }

        [Test]
        public void ExemptionAddedByACommand_WinsWithoutAnExit()
        {
            var session = new LevelSession(AsciiLevel.Parse(
                    "#####",
                    "#T..#",
                    "#####")
                .Block('T', 0, new FakeModifierData(() => new FakeBecomeExemptOnMove()))
                .Build());
            var t = (Block)session.Board.EntityAt(new Cell(1, 1));

            session.TryMove(t, Direction.Right);
            Assert.That(session.State, Is.EqualTo(GameState.Playing));

            session.CommitMove();
            Assert.That(session.State, Is.EqualTo(GameState.Won));
        }
    }
}
