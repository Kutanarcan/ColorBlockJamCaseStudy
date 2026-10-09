namespace Game.Runtime
{
    /// <summary>
    /// Why LoseLife is open (§5): HUD Restart asks to retry the level, Settings' Home asks to leave it. Same popup,
    /// different action label and action (D121).
    /// </summary>
    public enum LoseLifeVariant
    {
        Retry,
        Leave
    }
}
