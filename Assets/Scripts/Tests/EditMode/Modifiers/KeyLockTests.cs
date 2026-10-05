using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class KeyLockTests
    {
        private static readonly Cell LCell = new Cell(1, 1);
        private static readonly Cell MCell = new Cell(3, 1);
        private static readonly Cell KCell = new Cell(5, 1);

        // L and M can step right into a free cell; K sits above a door of its color.
        private static LevelSession NewSession(ModifierData[] l, ModifierData[] m, ModifierData[] k) =>
            new LevelSession(AsciiLevel.Parse(
                    "########",
                    "#L.M.K.#",
                    "#####1##")
                .Block('L', 1, l)
                .Block('M', 2, m)
                .Block('K', 0, k)
                .Door('1', 0, Direction.Down)
                .Build());

        private static Block BlockAt(LevelSession session, Cell cell) => (Block)session.Board.EntityAt(cell);

        private static ModifierData[] Mods(params ModifierData[] modifiers) => modifiers;

        [Test]
        public void KeyExit_OpensEveryLockWithItsKeyId()
        {
            LevelSession session = NewSession(
                Mods(new LockData { KeyId = 7 }),
                Mods(new LockData { KeyId = 7 }),
                Mods(new KeyData { KeyId = 7 }));
            Block l = BlockAt(session, LCell);
            Block m = BlockAt(session, MCell);
            Assert.That(session.TryMove(l, Direction.Right), Is.EqualTo(MoveResult.Blocked));

            session.TryMove(BlockAt(session, KCell), Direction.Down);

            Assert.That(session.TryMove(l, Direction.Right), Is.EqualTo(MoveResult.Moved));
            Assert.That(session.TryMove(m, Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void LockNeedingTwoKeys_StaysClosed_AfterOneKey()
        {
            LevelSession session = NewSession(
                Mods(new LockData { KeyId = 7, Count = 2 }),
                Mods(),
                Mods(new KeyData { KeyId = 7, Count = 1 }));

            session.TryMove(BlockAt(session, KCell), Direction.Down);

            Assert.That(session.TryMove(BlockAt(session, LCell), Direction.Right), Is.EqualTo(MoveResult.Blocked));
        }

        [Test]
        public void KeyCount_CountsAsSeveralKeys()
        {
            LevelSession session = NewSession(
                Mods(new LockData { KeyId = 7, Count = 2 }),
                Mods(),
                Mods(new KeyData { KeyId = 7, Count = 2 }));

            session.TryMove(BlockAt(session, KCell), Direction.Down);

            Assert.That(session.TryMove(BlockAt(session, LCell), Direction.Right), Is.EqualTo(MoveResult.Moved));
        }

        [Test]
        public void KeyWithAnotherKeyId_DoesNotOpen()
        {
            LevelSession session = NewSession(
                Mods(new LockData { KeyId = 8 }),
                Mods(),
                Mods(new KeyData { KeyId = 7 }));

            session.TryMove(BlockAt(session, KCell), Direction.Down);

            Assert.That(session.TryMove(BlockAt(session, LCell), Direction.Right), Is.EqualTo(MoveResult.Blocked));
        }
    }
}
