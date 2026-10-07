using System;

namespace Game.Runtime
{
    /// <summary>Who holds the input lock. Each lifts only its own, so a resume after a win keeps the board locked.</summary>
    [Flags]
    public enum InputLockReason
    {
        None = 0,
        Flow = 1,
        Pause = 2
    }
}
