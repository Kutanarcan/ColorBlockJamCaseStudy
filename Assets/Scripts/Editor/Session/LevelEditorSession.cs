using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// What the designer does, without Unity: the tool, the brush, the selection, strokes, resizing and undo.
    /// Every edit is recorded in the document first, so it can be undone. The open level, its file state and its
    /// rule results live in <see cref="LevelDocument"/>; edits of one entity's properties in <see cref="EntityEdits"/>.
    /// The window only draws this and forwards input.
    /// </summary>
    public sealed class LevelEditorSession
    {
        private readonly Stroke stroke = new Stroke();
        private LevelModel beforeStroke;
        private int selectedBeforeStroke;

        public LevelDocument Document { get; }
        public EntityEdits Edits { get; }
        public LevelModel Model => Document.Model;
        public Brush Brush { get; private set; } = new Brush(EntityKind.Block, 0, Direction.Down);
        public int Selected { get; private set; } = LevelModel.None;

        /// <summary>A stroke is open: mouse down happened, mouse up not yet.</summary>
        public bool IsStroking => stroke.IsActive;

        public LevelEditorSession(ModifierCatalog catalog, LevelRules rules)
        {
            Document = new LevelDocument(catalog, rules);
            Edits = new EntityEdits(Document, catalog);
        }

        /// <summary>A new level with a wall ring on the edge; paint doors over the ring.</summary>
        public void New(int width, int height, float timeLimit)
        {
            Document.Load(LevelTemplates.WalledLevel(width, height, timeLimit), "", "");
            Reset(Result.Success());
        }

        public Result Open(string key, string text) => Reset(Document.Open(key, text));

        public Result Restore(string key, string savedKey, string snapshot, bool wasDirty) =>
            Reset(Document.Restore(key, savedKey, snapshot, wasDirty));

        /// <summary>Choosing a brush creates nothing; it drops the selection, so the next stroke starts a new entity.</summary>
        public void ChooseBrush(Brush brush)
        {
            EndStroke();
            Brush = brush;
            Selected = LevelModel.None;
        }

        public void BeginStroke(Cell cell) => BeginStroke(cell, EditorTool.Paint);

        /// <summary>Mouse down on a cell: Paint for a left-drag, Erase for a right-drag. Ends a stroke left open first.</summary>
        public void BeginStroke(Cell cell, EditorTool tool)
        {
            EndStroke();
            beforeStroke = Model.Clone();
            selectedBeforeStroke = Selected;
            stroke.Begin(Model, tool, Brush, cell, Selected);
            FollowStroke();
        }

        public void ContinueStroke(Cell cell)
        {
            if (!stroke.IsActive)
                return;

            stroke.Continue(cell);
            FollowStroke();
        }

        /// <summary>Mouse up. A stroke is one edit: one undo step, and the rules run once.</summary>
        public void EndStroke()
        {
            if (!stroke.IsActive)
                return;

            stroke.End();

            if (!stroke.Changed)
                return;

            Document.Push(beforeStroke, selectedBeforeStroke);
            Document.Commit();
        }

        public void Select(int entity)
        {
            EndStroke();
            Selected = entity;
        }

        /// <summary>
        /// Ctrl/Cmd + click (D59): selects the entity on the cell and takes up its brush, so the next stroke started
        /// next to it extends it. An empty cell drops the selection.
        /// </summary>
        public void SelectAt(Cell cell)
        {
            int owner = Model.Contains(cell) ? Model.OwnerOf(cell) : LevelModel.None;
            Select(owner);

            if (owner == LevelModel.None)
                return;

            EntityKind kind = Model.KindOf(owner);
            Direction direction = kind == EntityKind.Door ? Model.DirectionOf(owner) : Brush.Direction;
            Brush = new Brush(kind, Model.ColorOf(owner), direction);
        }

        /// <summary>Esc: drops the selection, so the next stroke starts a new entity.</summary>
        public void Deselect() => Select(LevelModel.None);

        public void SetTimeLimit(float seconds)
        {
            Document.Record(Selected, "time");
            Model.TimeLimit = seconds;
            Document.Commit();
        }

        public void DeleteSelected()
        {
            if (Selected == LevelModel.None)
                return;

            Document.Record(Selected);
            Model.Delete(Selected);
            LevelFrame.Seal(Model);
            Selected = LevelModel.None;
            Document.Commit();
        }

        /// <summary>Grows (+1) or shrinks (-1) one side (D54). The window confirms a shrink that cuts content first.</summary>
        public void Resize(Direction side, int delta)
        {
            if (!LevelResizer.CanResize(Model, side, delta))
                return;

            EndStroke();
            Document.Record(Selected);
            Document.Replace(LevelResizer.Resize(Model, side, delta));
            DropLostSelection();
            Document.Commit();
        }

        public void Undo()
        {
            EndStroke();

            if (Document.Undo(Selected, out int restored))
                RestoreSelection(restored);
        }

        public void Redo()
        {
            EndStroke();

            if (Document.Redo(Selected, out int restored))
                RestoreSelection(restored);
        }

        private Result Reset(Result loaded)
        {
            if (loaded.IsSuccess)
            {
                stroke.End();
                Selected = LevelModel.None;
            }

            return loaded;
        }

        /// <summary>Paint selects the entity it paints into; Erase only drops a selection it erased away.</summary>
        private void FollowStroke()
        {
            if (stroke.Tool == EditorTool.Paint && stroke.Target != LevelModel.None)
                Selected = stroke.Target;
            else
                DropLostSelection();
        }

        private void RestoreSelection(int selected)
        {
            Selected = selected;
            DropLostSelection();
        }

        private void DropLostSelection()
        {
            if (Selected != LevelModel.None && !Model.Exists(Selected))
                Selected = LevelModel.None;
        }
    }
}
