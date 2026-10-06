using System.Collections.Generic;

namespace Game.LevelEditor
{
    /// <summary>
    /// Undo / redo by snapshots (D56). Each step holds the level as it was before an edit, with the selection.
    /// Snapshots keep entity ids (<see cref="LevelModel.Clone"/>), so a restored selection stays valid. At most
    /// <see cref="Capacity"/> steps are kept; the oldest goes first. A new edit clears the redo steps.
    /// </summary>
    public sealed class EditHistory
    {
        public const int Capacity = 100;

        private readonly List<Step> undo = new List<Step>();
        private readonly List<Step> redo = new List<Step>();
        private string mergeKey;

        public bool CanUndo => undo.Count > 0;
        public bool CanRedo => redo.Count > 0;

        /// <summary>
        /// Call before changing <paramref name="current"/>. Edits with the same non-null merge key in a row (typing
        /// into one field) are one step: only the first records.
        /// </summary>
        public void Record(LevelModel current, int selected, string key = null)
        {
            if (key != null && key == mergeKey && CanUndo)
                return;

            Push(current.Clone(), selected);
            mergeKey = key;
        }

        /// <summary>Records a copy taken earlier, e.g. at the start of a stroke that turned out to change the level.</summary>
        public void Push(LevelModel before, int selected)
        {
            if (undo.Count == Capacity)
                undo.RemoveAt(0);

            undo.Add(new Step(before, selected));
            redo.Clear();
            mergeKey = null;
        }

        /// <summary>Hands back the level before the last edit; <paramref name="current"/> becomes the redo step.</summary>
        public bool TryUndo(LevelModel current, int selected, out LevelModel model, out int restored) =>
            Move(undo, redo, current, selected, out model, out restored);

        public bool TryRedo(LevelModel current, int selected, out LevelModel model, out int restored) =>
            Move(redo, undo, current, selected, out model, out restored);

        public void Clear()
        {
            undo.Clear();
            redo.Clear();
            mergeKey = null;
        }

        private bool Move(List<Step> from, List<Step> to, LevelModel current, int selected,
            out LevelModel model, out int restored)
        {
            mergeKey = null;

            if (from.Count == 0)
            {
                model = null;
                restored = LevelModel.None;

                return false;
            }

            Step step = from[from.Count - 1];
            from.RemoveAt(from.Count - 1);
            to.Add(new Step(current, selected));
            model = step.Model;
            restored = step.Selected;

            return true;
        }

        private readonly struct Step
        {
            public LevelModel Model { get; }
            public int Selected { get; }

            public Step(LevelModel model, int selected)
            {
                Model = model;
                Selected = selected;
            }
        }
    }
}
