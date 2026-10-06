using System;
using UnityEngine;

namespace Game.LevelEditor
{
    /// <summary>
    /// Tints the buttons drawn inside a <c>using</c> block by what they do, and restores the previous tint after.
    /// A struct, so <c>using (ButtonTint.Danger())</c> does not allocate.
    /// </summary>
    internal readonly struct ButtonTint : IDisposable
    {
        private readonly Color previous;

        private ButtonTint(Color tint)
        {
            previous = GUI.backgroundColor;
            GUI.backgroundColor = tint;
        }

        /// <summary>The main action of its area: Save with changes, Add, the chosen brush kind.</summary>
        public static ButtonTint Primary() => new ButtonTint(new Color(0.45f, 0.70f, 1f));

        /// <summary>Adds room: grow a side.</summary>
        public static ButtonTint Positive() => new ButtonTint(new Color(0.50f, 0.90f, 0.55f));

        /// <summary>Removes something: delete, remove, shrink.</summary>
        public static ButtonTint Danger() => new ButtonTint(new Color(1f, 0.50f, 0.45f));

        /// <summary>No tint; for a choice that is not active.</summary>
        public static ButtonTint None() => new ButtonTint(Color.white);

        public void Dispose() => GUI.backgroundColor = previous;
    }
}
