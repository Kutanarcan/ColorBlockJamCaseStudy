using Game.Core;
using Game.LevelEditor;
using NUnit.Framework;

namespace Game.Tests.LevelEditor
{
    public class LevelEditorSessionTests
    {
        [Test]
        public void New_StartsWithAWallRing_SoEdgeRulesPass()
        {
            LevelEditorSession session = TestSessions.New(new EdgeCellsRule());

            Assert.That(session.Document.Violations, Is.Empty);
            Assert.That(session.Model.OwnerOf(new Cell(1, 1)), Is.EqualTo(LevelModel.None));
        }

        [Test]
        public void ChoosingABrush_CreatesNoEntity_UntilACellIsPainted()
        {
            LevelEditorSession session = TestSessions.New();
            int before = session.Model.EntityCount;

            session.ChooseBrush(new Brush(EntityKind.Wall, 0, Direction.Down));
            session.ChooseBrush(new Brush(EntityKind.Door, 1, Direction.Down));
            session.ChooseBrush(new Brush(EntityKind.Block, 2, Direction.Down));
            Assert.That(session.Model.EntityCount, Is.EqualTo(before));

            TestSessions.Click(session, new Cell(1, 1));
            Assert.That(session.Model.EntityCount, Is.EqualTo(before + 1));
        }

        [Test]
        public void Deselect_DropsTheSelection_SoTheNextStrokeStartsANewEntity()
        {
            LevelEditorSession session = TestSessions.Playable();
            int block = session.Selected;

            session.Deselect();
            TestSessions.Click(session, new Cell(3, 1));

            Assert.That(session.Model.OwnerOf(new Cell(3, 1)), Is.Not.EqualTo(block));
        }

        [Test]
        public void SelectAt_SelectsTheEntity_AndTakesUpItsBrush_SoAStrokeNextToItExtendsIt()
        {
            LevelEditorSession session = TestSessions.Playable();
            int block = session.Model.OwnerOf(new Cell(2, 1));
            session.ChooseBrush(new Brush(EntityKind.Wall, 0, Direction.Down));

            session.SelectAt(new Cell(2, 1));
            Assert.That(session.Selected, Is.EqualTo(block));
            Assert.That(session.Brush.Kind, Is.EqualTo(EntityKind.Block));
            Assert.That(session.Brush.ColorId, Is.EqualTo(1));

            TestSessions.Click(session, new Cell(3, 1));
            Assert.That(session.Model.OwnerOf(new Cell(3, 1)), Is.EqualTo(block));

            session.SelectAt(new Cell(2, 2));
            Assert.That(session.Selected, Is.EqualTo(LevelModel.None));
        }

        [Test]
        public void ChoosingABrush_DropsTheSelection()
        {
            LevelEditorSession session = TestSessions.Playable();
            Assert.That(session.Selected, Is.Not.EqualTo(LevelModel.None));

            session.ChooseBrush(new Brush(EntityKind.Block, 4, Direction.Down));

            Assert.That(session.Selected, Is.EqualTo(LevelModel.None));
        }

        [Test]
        public void PaintedEntity_IsSelected_AndTheLevelIsDirty()
        {
            LevelEditorSession session = TestSessions.New();
            session.ChooseBrush(new Brush(EntityKind.Block, 2, Direction.Down));

            TestSessions.Click(session, new Cell(1, 1));

            Assert.That(session.Model.OwnerOf(new Cell(1, 1)), Is.EqualTo(session.Selected));
            Assert.That(session.Document.IsDirty, Is.True);
        }

        [Test]
        public void Rules_RunWhenTheStrokeEnds_NotPerCell()
        {
            LevelEditorSession session = TestSessions.New(new DoorWidthRule());
            session.ChooseBrush(new Brush(EntityKind.Block, 2, Direction.Down));

            session.BeginStroke(new Cell(1, 1));
            Assert.That(session.Document.Violations, Is.Empty);

            session.EndStroke();
            Assert.That(session.Document.Violations.Count, Is.EqualTo(1));
        }

        [Test]
        public void PaintingADoorOverTheRing_TakesTheCellFromTheWall()
        {
            LevelEditorSession session = TestSessions.Playable();

            Assert.That(session.Model.KindOf(session.Model.OwnerOf(new Cell(2, 0))), Is.EqualTo(EntityKind.Door));
        }

        [Test]
        public void EraseStroke_ClearsTheCellAndTheSelection()
        {
            LevelEditorSession session = TestSessions.Playable();

            session.BeginStroke(new Cell(2, 1), EditorTool.Erase);
            session.EndStroke();
            Assert.That(session.Model.OwnerOf(new Cell(2, 1)), Is.EqualTo(LevelModel.None));
            Assert.That(session.Selected, Is.EqualTo(LevelModel.None));
        }


        [Test]
        public void Undo_RestoresTheLevelBeforeTheStroke_AndRedoReappliesIt()
        {
            LevelEditorSession session = TestSessions.Playable();
            int block = session.Model.OwnerOf(new Cell(2, 1));
            session.ChooseBrush(new Brush(EntityKind.Block, 3, Direction.Down));
            session.Select(block);
            session.BeginStroke(new Cell(1, 2));
            session.ContinueStroke(new Cell(3, 2));
            session.EndStroke();

            session.Undo();
            Assert.That(session.Model.OwnerOf(new Cell(2, 2)), Is.EqualTo(LevelModel.None));
            Assert.That(session.Selected, Is.EqualTo(block));

            session.Redo();
            Assert.That(session.Model.CellsOf(session.Selected).Count, Is.EqualTo(3));
        }

        [Test]
        public void StrokesThatChangeNothing_RecordNoUndoStep()
        {
            LevelEditorSession session = TestSessions.New();
            session.SelectAt(new Cell(1, 1));
            session.ChooseBrush(new Brush(EntityKind.Wall, 0, Direction.Down));
            TestSessions.Click(session, new Cell(0, 0));

            Assert.That(session.Document.CanUndo, Is.False);
        }

        [Test]
        public void TypingIntoOneField_UndoesAsOneStep()
        {
            LevelEditorSession session = TestSessions.New();

            session.SetTimeLimit(6f);
            session.SetTimeLimit(65f);
            session.Undo();

            Assert.That(session.Model.TimeLimit, Is.EqualTo(30f));
            Assert.That(session.Document.CanUndo, Is.False);
        }

        [Test]
        public void Resize_IsOneUndoStep()
        {
            LevelEditorSession session = TestSessions.Playable();

            session.Resize(Direction.Up, 1);
            Assert.That(session.Model.Height, Is.EqualTo(5));

            session.Undo();
            Assert.That(session.Model.Height, Is.EqualTo(4));
        }

        [Test]
        public void Shrink_ThatRemovesTheSelectedEntity_DropsTheSelection()
        {
            LevelEditorSession session = TestSessions.Playable();

            session.Resize(Direction.Down, -1);

            Assert.That(session.Selected, Is.EqualTo(LevelModel.None));
        }

        [Test]
        public void DeletingAFrameDoor_LeavesWallsInItsPlace()
        {
            LevelEditorSession session = TestSessions.Playable(new EdgeCellsRule());
            session.Select(session.Model.OwnerOf(new Cell(2, 0)));

            session.DeleteSelected();

            Assert.That(session.Model.KindOf(session.Model.OwnerOf(new Cell(2, 0))), Is.EqualTo(EntityKind.Wall));
            Assert.That(session.Document.Violations, Is.Empty);
        }

        [Test]
        public void DeleteSelected_WithNothingSelected_DoesNothing()
        {
            LevelEditorSession session = TestSessions.New();

            session.DeleteSelected();

            Assert.That(session.Document.CanUndo, Is.False);
        }

        [Test]
        public void Select_PicksAnEntityFromTheList()
        {
            LevelEditorSession session = TestSessions.Playable();
            int door = session.Model.OwnerOf(new Cell(2, 0));

            session.Select(door);

            Assert.That(session.Selected, Is.EqualTo(door));
        }
    }
}
