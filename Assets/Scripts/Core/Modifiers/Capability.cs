using System;

namespace Game.Core
{
    /// <summary>What the base block can do. Every block has all of them until a modifier suspends one.</summary>
    [Flags]
    public enum Capability
    {
        None = 0,
        Move = 1 << 0,
        Exit = 1 << 1
    }
}
