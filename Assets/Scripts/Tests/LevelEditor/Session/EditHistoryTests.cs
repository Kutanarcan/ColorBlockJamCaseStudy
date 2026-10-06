using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class EditHistoryTests
    {
        private readonly EditHistory history = new EditHistory();

        private static LevelModel Level(float timeLimit) => new LevelModel(3, 3) { TimeLimit = timeLimit };

        [Test]
        public void Undo_HandsBackTheRecordedLevel_AndRedoTheOneItReplaced()
        {
            LevelModel current = Level(10f);
            history.Record(current, 4);
            current.TimeLimit = 20f;

            Assert.That(history.TryUndo(current, 7, out LevelModel undone, out int undoneSelection), Is.True);
            Assert.That(undone.TimeLimit, Is.EqualTo(10f));
            Assert.That(undoneSelection, Is.EqualTo(4));

            Assert.That(history.TryRedo(undone, undoneSelection, out LevelModel redone, out int redoneSelection), Is.True);
            Assert.That(redone, Is.SameAs(current));
            Assert.That(redoneSelection, Is.EqualTo(7));
        }

        [Test]
        public void Record_CopiesTheLevel_SoLaterChangesStayOut()
        {
            LevelModel current = Level(10f);
            int block = current.AddBlock(0);
            current.Paint(new Cell(1, 1), block);
            history.Record(current, LevelModel.None);

            current.Erase(new Cell(1, 1));
            history.TryUndo(current, LevelModel.None, out LevelModel undone, out _);

            Assert.That(undone.OwnerOf(new Cell(1, 1)), Is.EqualTo(block));
        }

        [Test]
        public void ANewEdit_ClearsRedo()
        {
            history.Record(Level(10f), LevelModel.None);
            history.TryUndo(Level(20f), LevelModel.None, out _, out _);

            history.Record(Level(30f), LevelModel.None);

            Assert.That(history.CanRedo, Is.False);
        }

        [Test]
        public void TheSameMergeKeyInARow_IsOneStep()
        {
            history.Record(Level(10f), LevelModel.None, "time");
            history.Record(Level(11f), LevelModel.None, "time");

            history.TryUndo(Level(12f), LevelModel.None, out LevelModel undone, out _);

            Assert.That(undone.TimeLimit, Is.EqualTo(10f));
            Assert.That(history.CanUndo, Is.False);
        }

        [Test]
        public void AnotherKey_OrAnEditBetween_StartsANewStep()
        {
            history.Record(Level(10f), LevelModel.None, "time");
            history.Record(Level(11f), LevelModel.None, "color 1");
            history.Record(Level(12f), LevelModel.None);
            history.Record(Level(13f), LevelModel.None, "color 1");

            int steps = 0;
            LevelModel current = Level(14f);

            while (history.TryUndo(current, LevelModel.None, out current, out _))
            {
                steps++;
            }

            Assert.That(steps, Is.EqualTo(4));
        }

        [Test]
        public void AtCapacity_TheOldestStepIsDropped()
        {
            for (int i = 0; i <= EditHistory.Capacity; i++)
            {
                history.Record(Level(i), LevelModel.None);
            }

            LevelModel current = Level(-1f);

            while (history.TryUndo(current, LevelModel.None, out LevelModel undone, out _))
            {
                current = undone;
            }

            Assert.That(current.TimeLimit, Is.EqualTo(1f));
        }

        [Test]
        public void Undo_WithNothingRecorded_Fails()
        {
            Assert.That(history.TryUndo(Level(1f), LevelModel.None, out _, out _), Is.False);
        }
    }
}
