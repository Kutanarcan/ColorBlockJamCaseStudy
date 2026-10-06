using System.Collections.Generic;
using Game.Core;
using UnityEditor;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// The selected entity: color, direction and modifiers. Modifier fields are drawn generically from the DTO's
    /// own Write / Read (ModifierFields), so a new modifier needs no drawer.
    /// </summary>
    internal sealed class SelectionPanel
    {
        private readonly LevelEditorSession session;
        private readonly string[] modifierNames;
        private int modifierChoice;

        public SelectionPanel(LevelEditorSession session)
        {
            this.session = session;
            modifierNames = ToArray(session.Edits.ModifierNames);
        }

        public void Draw()
        {
            EditorTheme.BeginSection("Selected");

            if (session.Selected == LevelModel.None)
                GUILayout.Label("Paint with the brush, or Ctrl/Cmd + click an entity.", EditorTheme.Note);
            else
                DrawEntity(session.Selected);

            EditorTheme.EndSection();
        }

        private void DrawEntity(int selected)
        {
            LevelModel model = session.Model;
            EntityKind kind = model.KindOf(selected);

            EditorGUILayout.BeginHorizontal();
            EditorTheme.Chip(kind == EntityKind.Wall ? PreviewColors.Wall : PreviewColors.Of(model.ColorOf(selected)));
            GUILayout.Label($"{kind} {model.OrdinalOf(selected)} of {model.CountOf(kind)}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{model.CellsOf(selected).Count} cell(s)", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            if (kind != EntityKind.Wall)
            {
                int color = ColorSwatches.Draw("Color", model.ColorOf(selected));
                if (color != model.ColorOf(selected)) session.Edits.SetColor(selected, color);
            }

            if (kind == EntityKind.Door)
            {
                var direction = (Direction)EditorGUILayout.EnumPopup("Direction", model.DirectionOf(selected));
                if (direction != model.DirectionOf(selected)) session.Edits.SetDirection(selected, direction);
            }

            if (kind == EntityKind.Block)
                DrawModifiers(model.ModifiersOf(selected));

            EditorGUILayout.Space();
            using (ButtonTint.Danger())
            {
                if (GUILayout.Button("Delete entity")) session.DeleteSelected();
            }
        }

        private void DrawModifiers(IReadOnlyList<ModifierData> modifiers)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);

            for (int i = 0; i < modifiers.Count; i++)
            {
                if (DrawModifier(modifiers[i], i))
                {
                    session.Edits.RemoveModifier(session.Selected, i);

                    return;
                }
            }

            if (modifierNames.Length == 0)
                return;

            EditorGUILayout.BeginHorizontal();
            modifierChoice = EditorGUILayout.Popup(Mathf.Clamp(modifierChoice, 0, modifierNames.Length - 1), modifierNames);
            using (ButtonTint.Primary())
            {
                if (GUILayout.Button("Add", GUILayout.Width(50f)))
                    session.Edits.AddModifier(session.Selected, modifierNames[modifierChoice]);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>Draws one modifier's fields; returns true when it should be removed.</summary>
        private bool DrawModifier(ModifierData modifier, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(modifier.TypeName, EditorStyles.boldLabel);
            bool remove;

            using (ButtonTint.Danger())
            {
                remove = GUILayout.Button("Remove", GUILayout.Width(70f));
            }
            EditorGUILayout.EndHorizontal();

            ModifierFields fields = ModifierFields.Of(modifier);
            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < fields.Count; i++)
            {
                DrawField(fields, i);
            }

            if (EditorGUI.EndChangeCheck())
                session.Edits.EditModifier(session.Selected, index, fields);

            EditorGUILayout.EndVertical();

            return remove;
        }

        // A new field kind (LevelFormat.md §4.2) adds a case here.
        private static void DrawField(ModifierFields fields, int index)
        {
            switch (fields.KindAt(index))
            {
                case FieldKind.Int:
                    fields.SetInt(index, EditorGUILayout.IntField(fields.KeyAt(index), fields.IntAt(index)));
                    break;
                case FieldKind.Direction:
                    fields.SetDirection(index,
                        (Direction)EditorGUILayout.EnumPopup(fields.KeyAt(index), fields.DirectionAt(index)));
                    break;
                case FieldKind.Axis:
                    fields.SetAxis(index, (Axis)EditorGUILayout.EnumPopup(fields.KeyAt(index), fields.AxisAt(index)));
                    break;
            }
        }

        private static string[] ToArray(IReadOnlyList<string> list)
        {
            var array = new string[list.Count];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = list[i];
            }

            return array;
        }
    }
}
