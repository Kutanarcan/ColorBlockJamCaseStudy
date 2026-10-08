namespace Game.Infrastructure
{
    /// <summary>
    /// Which level the next Gameplay scene plays. The start scene's services pick the implementation (D131): the game
    /// follows progression.
    /// </summary>
    public interface ILevelChoice
    {
        string Choose();
    }
}
