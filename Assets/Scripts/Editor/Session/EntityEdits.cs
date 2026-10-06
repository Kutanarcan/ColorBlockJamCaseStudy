using System.Collections.Generic;
using Game.Core;

namespace Game.LevelEditor
{
    /// <summary>
    /// Edits of one entity's properties: color, direction, modifiers. Each is one undo step; typing into one field
    /// of one entity merges into a single step. A modifier is never changed in place, only replaced, so undo
    /// snapshots can share modifier instances.
    /// </summary>
    public sealed class EntityEdits
    {
        private readonly LevelDocument document;
        private readonly ModifierCatalog catalog;

        public EntityEdits(LevelDocument document, ModifierCatalog catalog)
        {
            this.document = document;
            this.catalog = catalog;
        }

        public IReadOnlyList<string> ModifierNames => catalog.Names;

        private LevelModel Model => document.Model;

        public void SetColor(int entity, int colorId)
        {
            document.Record(entity, $"color {entity}");
            Model.SetColor(entity, colorId);
            document.Commit();
        }

        public void SetDirection(int entity, Direction direction)
        {
            document.Record(entity);
            Model.SetDirection(entity, direction);
            document.Commit();
        }

        public void AddModifier(int entity, string name)
        {
            if (!catalog.TryCreate(name, out ModifierData data))
                return;

            document.Record(entity);
            Model.AddModifier(entity, data);
            document.Commit();
        }

        public void RemoveModifier(int entity, int index)
        {
            document.Record(entity);
            Model.RemoveModifierAt(entity, index);
            document.Commit();
        }

        /// <summary>Applies edited fields (the DTO's own Write / Read) as a new instance of the same modifier type.</summary>
        public void EditModifier(int entity, int index, ModifierFields fields)
        {
            if (!catalog.TryCreate(Model.ModifiersOf(entity)[index].TypeName, out ModifierData edited))
                return;

            fields.ApplyTo(edited);
            document.Record(entity, $"modifier {entity} {index}");
            Model.ReplaceModifierAt(entity, index, edited);
            document.Commit();
        }
    }
}
