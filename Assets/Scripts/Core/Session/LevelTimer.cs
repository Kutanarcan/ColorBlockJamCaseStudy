namespace Game.Core
{
    /// <summary>Counts down in seconds and never goes below zero.</summary>
    public sealed class LevelTimer
    {
        public float Remaining { get; private set; }
        public bool IsExpired => Remaining <= 0f;

        public void Reset(float seconds) => Remaining = seconds;

        public void Add(float seconds) => Remaining += seconds;

        public void Tick(float deltaTime)
        {
            Remaining -= deltaTime;

            if (Remaining < 0f)
                Remaining = 0f;
        }
    }
}
