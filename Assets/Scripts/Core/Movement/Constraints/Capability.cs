using System;

namespace Game.Core
{
    /// <summary>
    /// What a base block can do: Move and Exit, until a modifier suspends one.
    /// Modifiers list what they suspend explicitly; there is no "all" value.
    /// </summary>
    [Flags]
    public enum Capability
    {
        None = 0,
        Move = 1 << 0,
        Exit = 1 << 1
    }
}
