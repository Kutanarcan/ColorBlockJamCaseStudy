namespace Game.Runtime
{
    /// <summary>How loudly the HUD timer warns (D143).</summary>
    public enum TimerUrgency
    {
        /// <summary>Plenty of time: no beat.</summary>
        None,

        /// <summary>Below the warning time: the timer beats every second.</summary>
        Warning,

        /// <summary>The last seconds: every beat also flashes the alarm red.</summary>
        Critical
    }
}
