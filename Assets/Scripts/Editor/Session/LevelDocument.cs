using System.Collections.Generic;
using Game.Core;
using Game.LevelIO;

namespace Game.LevelEditor
{
    /// <summary>
    /// The open level: its model, its key and saved state, its undo history and its rule results. A change is
    /// <see cref="Record"/> before and <see cref="Commit"/> after: one undo step, the level marked dirty, the rules
    /// run once.
    /// </summary>
    public sealed class LevelDocument
    {
        private readonly LevelJson json;
        private readonly RuleReport report;
        private readonly EditHistory history = new EditHistory();

        public LevelModel Model { get; private set; }
        public string Key { get; set; } = "";

        /// <summary>The key whose file holds this level, "" until it is opened or saved. Saving under it needs no overwrite question.</summary>
        public string SavedKey { get; private set; } = "";

        public bool IsDirty { get; private set; }
        public bool CanUndo => history.CanUndo;
        public bool CanRedo => history.CanRedo;
        public IReadOnlyList<RuleViolation> Violations => report.Violations;

        public LevelDocument(ModifierCatalog catalog, LevelRules rules)
        {
            json = new LevelJson(catalog);
            report = new RuleReport(rules);
        }

        public void Load(LevelModel model, string key, string savedKey)
        {
            Model = model;
            Key = key;
            SavedKey = savedKey;
            IsDirty = false;
            history.Clear();
            report.Refresh(model);
        }

        /// <summary>Opens a level file's text; on failure the current level stays.</summary>
        public Result Open(string key, string text) => Restore(key, key, text, false);

        /// <summary>Brings back a snapshot after a domain reload, keeping its unsaved state.</summary>
        public Result Restore(string key, string savedKey, string text, bool wasDirty)
        {
            Result<LevelData> parsed = json.Parse(text);

            if (parsed.IsFailure)
                return Result.Failure(parsed.Error);

            Result<LevelModel> model = LevelModelConverter.FromLevelData(parsed.Value);

            if (model.IsFailure)
                return Result.Failure(model.Error);

            Load(model.Value, key, savedKey);
            IsDirty = wasDirty;

            return Result.Success();
        }

        /// <summary>The JSON to write under <see cref="Key"/>; fails while the key is invalid or a rule is broken.</summary>
        public Result<string> Save()
        {
            if (!LevelKey.IsValid(Key))
                return Result<string>.Failure("The level key may only use letters, digits, '-' and '_'.");

            report.Refresh(Model);

            if (Violations.Count > 0)
                return Result<string>.Failure($"{Violations.Count} rule(s) broken; fix them before saving.");

            return Result<string>.Success(Snapshot());
        }

        public void MarkSaved()
        {
            IsDirty = false;
            SavedKey = Key;
        }

        /// <summary>The level as JSON regardless of rules, to survive a domain reload.</summary>
        public string Snapshot() => json.Serialize(LevelModelConverter.ToLevelData(Model));

        public bool IsFlagged(int cellIndex) => report.IsFlagged(cellIndex);

        /// <summary>Call before a change. Changes with the same merge key in a row (typing into one field) undo as one step.</summary>
        public void Record(int selected, string mergeKey = null) => history.Record(Model, selected, mergeKey);

        /// <summary>Records a copy taken before a stroke, once its end shows it changed the level.</summary>
        public void Push(LevelModel before, int selected) => history.Push(before, selected);

        /// <summary>Swaps in a rebuilt level (a resize). Call between Record and Commit.</summary>
        public void Replace(LevelModel model) => Model = model;

        public void Commit()
        {
            IsDirty = true;
            report.Refresh(Model);
        }

        /// <returns>False when there is nothing to undo; otherwise the selection to restore.</returns>
        public bool Undo(int selected, out int restored)
        {
            if (!history.TryUndo(Model, selected, out LevelModel model, out restored))
                return false;

            Model = model;
            Commit();

            return true;
        }

        public bool Redo(int selected, out int restored)
        {
            if (!history.TryRedo(Model, selected, out LevelModel model, out restored))
                return false;

            Model = model;
            Commit();

            return true;
        }
    }
}
