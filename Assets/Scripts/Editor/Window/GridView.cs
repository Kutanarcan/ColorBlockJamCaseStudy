using System.Collections.Generic;
using System.Text;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>Draws the grid (row 0 at the bottom) and forwards input: left-drag uses the tool, right-drag erases, Ctrl/Cmd + click selects.</summary>
    internal sealed class GridView
    {
        private const float CellSize = 30f;
        private const float Border = 2f;

        /// <summary>
        /// Matches the grid to its control id by this hint, not by how many controls came before it. The panels are
        /// drawn first and change size when the selection changes (a first click selects what it paints), which would
        /// otherwise shift the id between mouse down and drag, and the drag would be lost.
        /// </summary>
        private static readonly int ControlHint = "Game.LevelEditor.GridView".GetHashCode();

        private readonly LevelEditorSession session;
        private readonly StringBuilder label = new StringBuilder();

        public GridView(LevelEditorSession session) => this.session = session;

        /// <summary>Returns true when input changed the level, so the window repaints.</summary>
        public bool Draw()
        {
            LevelModel model = session.Model;
            float padding = EditorTheme.GridPadding;
            Rect board = GUILayoutUtility.GetRect(model.Width * CellSize + 2f * padding,
                model.Height * CellSize + 2f * padding, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            Rect area = Inset(board, padding);

            if (Event.current.type == EventType.Repaint)
                DrawCells(model, area);

            return HandleMouse(model, area);
        }

        private void DrawCells(LevelModel model, Rect area)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    DrawCell(model, new Cell(x, y), CellRect(model, area, x, y));
                }
            }
        }

        private void DrawCell(LevelModel model, Cell cell, Rect rect)
        {
            int owner = model.OwnerOf(cell);
            EditorGUI.DrawRect(rect, FillOf(model, owner));

            if (owner != LevelModel.None)
                GUI.Label(rect, LabelOf(model, owner, cell), EditorStyles.centeredGreyMiniLabel);

            // Selection and violation can share a cell (a rule flags a block's first cell): selection outside,
            // violation inset, so neither hides the other.
            if (owner != LevelModel.None && owner == session.Selected)
                DrawOutline(rect, PreviewColors.Selection);

            if (session.Document.IsFlagged(model.IndexOf(cell)))
                DrawOutline(Inset(rect, Border), PreviewColors.Violation);
        }

        private static Color FillOf(LevelModel model, int owner)
        {
            if (owner == LevelModel.None)
                return PreviewColors.Empty;

            switch (model.KindOf(owner))
            {
                case EntityKind.Wall: return PreviewColors.Wall;
                case EntityKind.Door: return Color.Lerp(PreviewColors.Of(model.ColorOf(owner)), Color.black, 0.45f);
                default: return PreviewColors.Of(model.ColorOf(owner));
            }
        }

        /// <summary>Doors show their direction; a block's first cell shows its modifiers' initials.</summary>
        private string LabelOf(LevelModel model, int owner, Cell cell)
        {
            EntityKind kind = model.KindOf(owner);

            if (kind == EntityKind.Door)
                return ArrowOf(model.DirectionOf(owner));

            if (kind != EntityKind.Block || model.CellsOf(owner)[0] != model.IndexOf(cell))
                return "";

            label.Clear();
            IReadOnlyList<ModifierData> modifiers = model.ModifiersOf(owner);

            for (int i = 0; i < modifiers.Count; i++)
            {
                label.Append(char.ToUpperInvariant(modifiers[i].TypeName[0]));
            }

            return label.ToString();
        }

        private static string ArrowOf(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return "^";
                case Direction.Down: return "v";
                case Direction.Left: return "<";
                default: return ">";
            }
        }

        /// <summary>
        /// Mouse down, drag, up → one stroke. Events are read as they are and the open stroke decides, not
        /// <c>GetTypeForControl</c>: while another control is still hot (a panel clicked just before), it reports every
        /// mouse event as Ignore, so the first click and the closing mouse up were lost, and an unclosed stroke left
        /// the rules unrefreshed. The grid still takes the hot control, so the drag and the mouse up reach it outside
        /// the grid, and drops keyboard focus, so a field being edited lets go.
        /// </summary>
        private bool HandleMouse(LevelModel model, Rect area)
        {
            int control = GUIUtility.GetControlID(ControlHint, FocusType.Passive);
            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0 && EditorGUI.actionKey && area.Contains(e.mousePosition):
                    session.SelectAt(CellAt(model, area, e.mousePosition));
                    break;
                case EventType.MouseDown when e.button <= 1 && area.Contains(e.mousePosition):
                    GUIUtility.hotControl = control;
                    GUIUtility.keyboardControl = 0;
                    session.BeginStroke(CellAt(model, area, e.mousePosition), e.button == 1 ? EditorTool.Erase : EditorTool.Paint);
                    break;
                case EventType.MouseDrag when session.IsStroking:
                    session.ContinueStroke(CellAt(model, area, e.mousePosition));
                    break;
                case EventType.MouseUp when session.IsStroking:
                    if (GUIUtility.hotControl == control)
                        GUIUtility.hotControl = 0;

                    session.EndStroke();
                    break;
                default:
                    return false;
            }

            e.Use();

            return true;
        }

        private static Cell CellAt(LevelModel model, Rect area, Vector2 position) =>
            new Cell(Mathf.FloorToInt((position.x - area.x) / CellSize),
                model.Height - 1 - Mathf.FloorToInt((position.y - area.y) / CellSize));

        private static Rect CellRect(LevelModel model, Rect area, int x, int y) =>
            new Rect(area.x + x * CellSize, area.y + (model.Height - 1 - y) * CellSize, CellSize - 1f, CellSize - 1f);

        private static Rect Inset(Rect rect, float by) =>
            new Rect(rect.x + by, rect.y + by, rect.width - 2f * by, rect.height - 2f * by);

        private static void DrawOutline(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, Border), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - Border, rect.width, Border), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, Border, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - Border, rect.y, Border, rect.height), color);
        }
    }
}
