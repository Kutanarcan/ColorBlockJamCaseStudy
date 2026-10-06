using System;
using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>
    /// Every modifier a level may contain, by saved name. An explicit list instead of reflection discovery,
    /// so nothing is stripped or looked up by name under IL2CPP.
    /// </summary>
    public sealed class ModifierCatalog
    {
        private readonly Dictionary<string, Func<ModifierData>> factories = new Dictionary<string, Func<ModifierData>>();
        private readonly List<string> names = new List<string>();

        // A new modifier adds one line here.
        public static ModifierCatalog Default() =>
            new ModifierCatalog()
                .Add(() => new IceData())
                .Add(() => new ArrowData());

        public ModifierCatalog Add(Func<ModifierData> factory)
        {
            string name = factory().TypeName;

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A modifier needs a type name.", nameof(factory));

            if (factories.ContainsKey(name))
                throw new InvalidOperationException($"Modifier type '{name}' is added twice.");

            factories.Add(name, factory);
            names.Add(name);

            return this;
        }

        /// <summary>Saved names in the order they were added (the editor's "add modifier" list).</summary>
        public IReadOnlyList<string> Names => names;

        public bool Contains(string name) => factories.ContainsKey(name);

        public bool TryCreate(string name, out ModifierData data)
        {
            if (!factories.TryGetValue(name, out Func<ModifierData> factory))
            {
                data = null;

                return false;
            }

            data = factory();

            return true;
        }
    }
}
