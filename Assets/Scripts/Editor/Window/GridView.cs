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

        private readonly LevelEditorSession session;
        private readonly StringBuilder label = new StringBuilder();

        public GridView(LevelEditorSession session) => this.session = session;

        /// <summary>Returns true when input changed the level, so the window repaints.</summary>
        public bool Draw()
        {
            LevelModel model = session.Model;
            Rect area = GUILayoutUtility.GetRect(model.Width * CellSize, model.Height * CellSize,
                GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

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
        /// Mouse down, drag, up → one stroke. The grid takes the hot control on mouse down, so the drag and the
        /// mouse up still reach it outside the grid.
        /// </summary>
        private bool HandleMouse(LevelModel model, Rect area)
        {
            int control = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;

            switch (e.GetTypeForControl(control))
            {
                case EventType.MouseDown when e.button == 0 && EditorGUI.actionKey && area.Contains(e.mousePosition):
                    session.SelectAt(CellAt(model, area, e.mousePosition));
                    break;
                case EventType.MouseDown when e.button <= 1 && area.Contains(e.mousePosition):
                    GUIUtility.hotControl = control;
                    session.BeginStroke(CellAt(model, area, e.mousePosition), e.button == 1 ? EditorTool.Erase : EditorTool.Paint);
                    break;
                case EventType.MouseDrag when GUIUtility.hotControl == control:
                    session.ContinueStroke(CellAt(model, area, e.mousePosition));
                    break;
                case EventType.MouseUp when GUIUtility.hotControl == control:
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
