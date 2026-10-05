using System;

namespace Game.Core
{
    /// <summary>
    /// What a base entity can do. A block has Move and Exit, a door has Accept, until a modifier suspends one.
    /// Modifiers list what they suspend explicitly; there is no "all" value.
    /// </summary>
    [Flags]
    public enum Capability
    {
        None = 0,
        Move = 1 << 0,
        Exit = 1 << 1,
        Accept = 1 << 2
    }
}
