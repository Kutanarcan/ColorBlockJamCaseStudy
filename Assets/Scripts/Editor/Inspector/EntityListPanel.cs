using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>Every existing entity, grouped by kind; clicking a row selects the entity.</summary>
    internal sealed class EntityListPanel
    {
        private static readonly EntityKind[] Kinds = { EntityKind.Wall, EntityKind.Door, EntityKind.Block };
        private static readonly string[] GroupNames = { "Walls", "Doors", "Blocks" };

        private readonly LevelEditorSession session;
        private readonly List<int> entities = new List<int>();
        private readonly bool[] expanded = { false, true, true };

        public EntityListPanel(LevelEditorSession session) => this.session = session;

        public void Draw()
        {
            if (EditorTheme.BeginFoldoutSection("Entities"))
                DrawGroups();

            EditorTheme.EndSection();
        }

        private void DrawGroups()
        {
            for (int group = 0; group < Kinds.Length; group++)
            {
                session.Model.CollectEntities(Kinds[group], entities);
                expanded[group] = EditorGUILayout.Foldout(expanded[group], $"{GroupNames[group]} ({entities.Count})", true);

                if (!expanded[group])
                    continue;

                for (int i = 0; i < entities.Count; i++)
                {
                    DrawRow(entities[i], i + 1);
                }
            }
        }

        private void DrawRow(int entity, int ordinal)
        {
            LevelModel model = session.Model;
            EntityKind kind = model.KindOf(entity);
            bool selected = entity == session.Selected;
            Rect row = EditorGUILayout.BeginHorizontal();

            if (selected && Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(row, EditorTheme.Accent * new Color(1f, 1f, 1f, 0.35f));

            GUILayout.Space(4f);
            EditorTheme.Chip(kind == EntityKind.Wall ? PreviewColors.Wall : PreviewColors.Of(model.ColorOf(entity)));

            string label = kind == EntityKind.Door
                ? $"{kind} {ordinal}   {model.DirectionOf(entity)} · {model.CellsOf(entity).Count} cell(s)"
                : $"{kind} {ordinal}   {model.CellsOf(entity).Count} cell(s)";

            if (GUILayout.Button(label, EditorStyles.label) && !selected)
                session.Select(entity);

            EditorGUILayout.EndHorizontal();
        }
    }
}
