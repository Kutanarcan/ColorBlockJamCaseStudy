using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// One drag of a tool over the grid, mouse down to mouse up. A Paint stroke that starts on an entity matching
    /// the brush, or right next to the selected entity when it matches, extends that entity (D59); anywhere else it
    /// paints a new entity over whatever is there, created only when its first cell is painted (D53). Cells outside
    /// the grid are skipped.
    /// </summary>
    public sealed class Stroke
    {
        private static readonly Cell[] Neighbours = { new Cell(1, 0), new Cell(-1, 0), new Cell(0, 1), new Cell(0, -1) };

        private readonly List<Cell> path = new List<Cell>();
        private LevelModel model;
        private Brush brush;
        private Cell last;

        public bool IsActive { get; private set; }

        /// <summary>The stroke's own tool: a right-drag erases whatever tool is chosen.</summary>
        public EditorTool Tool { get; private set; }

        /// <summary>True once the stroke has changed the level.</summary>
        public bool Changed { get; private set; }

        /// <summary>The entity Paint paints into; None for Erase, or until Paint creates one.</summary>
        public int Target { get; private set; } = LevelModel.None;

        /// <param name="selected">The selected entity, extended when the stroke starts next to it; None for none.</param>
        public void Begin(LevelModel level, EditorTool activeTool, Brush activeBrush, Cell cell, int selected)
        {
            model = level;
            Tool = activeTool;
            brush = activeBrush;
            last = cell;
            IsActive = true;
            Changed = false;
            Target = StartTarget(cell, selected);
            Apply(cell);
        }

        /// <summary>Applies the tool to every cell from the previous drag point to this one.</summary>
        public void Continue(Cell cell)
        {
            if (!IsActive || cell == last)
                return;

            CellLine.Fill(last, cell, path);

            for (int i = 0; i < path.Count; i++)
            {
                Apply(path[i]);
            }

            last = cell;
        }

        public void End() => IsActive = false;

        private int StartTarget(Cell cell, int selected)
        {
            if (Tool != EditorTool.Paint || !model.Contains(cell))
                return LevelModel.None;

            int owner = model.OwnerOf(cell);

            if (owner != LevelModel.None && brush.Matches(model, owner, cell))
                return owner;

            return selected != LevelModel.None && brush.Matches(model, selected, cell) && Touches(selected, cell)
                ? selected
                : LevelModel.None;
        }

        private bool Touches(int entity, Cell cell)
        {
            foreach (Cell offset in Neighbours)
            {
                Cell next = cell + offset;

                if (model.Contains(next) && model.OwnerOf(next) == entity)
                    return true;
            }

            return false;
        }

        private void Apply(Cell cell)
        {
            if (!model.Contains(cell))
                return;

            if (Tool == EditorTool.Paint)
                Paint(cell);
            else
                Erase(cell);
        }

        /// <summary>A block brush skips the frame (D58).</summary>
        private void Paint(Cell cell)
        {
            if (!LevelFrame.Allows(model, cell, brush.Kind))
                return;

            if (Target == LevelModel.None)
                Target = brush.CreateIn(model, cell);

            if (model.OwnerOf(cell) == Target)
                return;

            model.Paint(cell, Target);
            Changed = true;
        }

        /// <summary>Erasing a frame cell turns it back into a wall (D58).</summary>
        private void Erase(Cell cell)
        {
            int owner = model.OwnerOf(cell);

            if (owner == LevelModel.None)
                return;

            if (!EdgeCells.IsEdge(model, cell))
                model.Erase(cell);
            else if (model.KindOf(owner) != EntityKind.Wall)
                model.Paint(cell, LevelFrame.WallFor(model, cell));
            else
                return;

            Changed = true;
        }
    }
}
