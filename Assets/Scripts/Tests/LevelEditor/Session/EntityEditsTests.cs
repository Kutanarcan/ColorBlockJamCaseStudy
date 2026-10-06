using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class EntityEditsTests
    {
        [Test]
        public void EditingAModifier_ReplacesIt_SoUndoBringsTheOldValueBack()
        {
            LevelEditorSession session = TestSessions.Playable();
            int block = session.Selected;
            session.Edits.AddModifier(block, "ice");
            ModifierData original = session.Model.ModifiersOf(block)[0];
            ModifierFields fields = ModifierFields.Of(original);
            fields.SetInt(0, 5);

            session.Edits.EditModifier(block, 0, fields);
            Assert.That(((IceData)session.Model.ModifiersOf(block)[0]).Count, Is.EqualTo(5));
            Assert.That(((IceData)original).Count, Is.EqualTo(0));

            session.Undo();
            Assert.That(session.Model.ModifiersOf(block)[0], Is.SameAs(original));
        }

        [Test]
        public void TypingAColor_OnOneEntity_UndoesAsOneStep_ButNotAcrossEntities()
        {
            LevelEditorSession session = TestSessions.Playable();
            int block = session.Model.OwnerOf(new Cell(2, 1));
            int door = session.Model.OwnerOf(new Cell(2, 0));

            session.Edits.SetColor(block, 1);
            session.Edits.SetColor(block, 12);
            session.Edits.SetColor(door, 12);

            session.Undo();
            Assert.That(session.Model.ColorOf(door), Is.EqualTo(1));
            Assert.That(session.Model.ColorOf(block), Is.EqualTo(12));

            session.Undo();
            Assert.That(session.Model.ColorOf(block), Is.EqualTo(1));
        }

        [Test]
        public void AddModifier_WithAnUnknownName_RecordsNothing()
        {
            LevelEditorSession session = TestSessions.Playable();
            session.Undo();
            int door = session.Model.OwnerOf(new Cell(2, 0));

            session.Edits.AddModifier(door, "no-such-modifier");

            Assert.That(session.Model.ModifiersOf(door), Is.Empty);
            Assert.That(session.Document.CanRedo, Is.True);
        }
    }
}
