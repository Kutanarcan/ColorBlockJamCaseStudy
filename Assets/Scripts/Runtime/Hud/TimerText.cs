using System;

namespace Game.Runtime
{
    /// <summary>
    /// The countdown as the HUD shows it, <c>mm:ss</c>. Seconds round up, so the timer shows <c>00:00</c> only once the
    /// time is really out (the level fails at 0) and a full limit shows whole (<c>05:00</c>, not <c>04:59</c>).
    /// Never below zero.
    /// </summary>
    public readonly struct TimerText : IEquatable<TimerText>
    {
        public readonly int Minutes;
        public readonly int Seconds;

        private TimerText(int totalSeconds)
        {
            Minutes = totalSeconds / 60;
            Seconds = totalSeconds % 60;
        }

        public static TimerText From(float remainingSeconds) =>
            new TimerText(remainingSeconds > 0f ? (int)Math.Ceiling(remainingSeconds) : 0);

        public bool Equals(TimerText other) => Minutes == other.Minutes && Seconds == other.Seconds;

        public override bool Equals(object obj) => obj is TimerText other && Equals(other);

        public override int GetHashCode() => Minutes * 60 + Seconds;
    }
}
