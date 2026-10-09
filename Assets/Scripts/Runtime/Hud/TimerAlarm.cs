using System;

namespace Game.Runtime
{
    /// <summary>
    /// When the HUD timer warns (D143): below the warning time it beats every second, and within the critical time
    /// each beat also flashes red. Reads the time as the HUD shows it, so a beat lands as the number changes. At zero
    /// nothing beats: the level has just failed.
    /// </summary>
    public sealed class TimerAlarm
    {
        private readonly int warningSeconds;
        private readonly int criticalSeconds;

        public TimerAlarm(int warningSeconds, int criticalSeconds)
        {
            if (criticalSeconds < 0 || criticalSeconds > warningSeconds)
                throw new ArgumentOutOfRangeException(nameof(criticalSeconds),
                    "The critical time is between zero and the warning time.");

            this.warningSeconds = warningSeconds;
            this.criticalSeconds = criticalSeconds;
        }

        public TimerUrgency UrgencyOf(TimerText shown)
        {
            int seconds = shown.TotalSeconds;

            if (seconds <= 0)
                return TimerUrgency.None;

            if (seconds <= criticalSeconds)
                return TimerUrgency.Critical;

            return seconds < warningSeconds ? TimerUrgency.Warning : TimerUrgency.None;
        }
    }
}
