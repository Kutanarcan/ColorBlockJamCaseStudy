namespace Game.Runtime
{
    /// <summary>The director's flag that keeps the player's hands off the board (D69); the drag reads it.</summary>
    public sealed class InputLock
    {
        public bool IsLocked { get; private set; }

        public void Lock() => IsLocked = true;

        public void Unlock() => IsLocked = false;
    }
}
