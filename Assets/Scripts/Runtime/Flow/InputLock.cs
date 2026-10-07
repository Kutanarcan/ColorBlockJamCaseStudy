namespace Game.Runtime
{
    /// <summary>
    /// Keeps the player's hands off the board (D69, D70); the drag reads it. Locked while any reason holds it:
    /// the director's flow (win, fail) or the pause.
    /// </summary>
    public sealed class InputLock
    {
        private InputLockReason reasons;

        public bool IsLocked => reasons != InputLockReason.None;

        public void Lock(InputLockReason reason) => reasons |= reason;

        public void Unlock(InputLockReason reason) => reasons &= ~reason;
    }
}
